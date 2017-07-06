using UnityEngine;
using FFmpeg.AutoGen;
using System;
using System.Diagnostics;
#if !UNITY_EDITOR && UNITY_METRO
using System.Threading.Tasks;
#endif

public unsafe class VideoDecoder : MonoBehaviour
{
    // var url = @"";
    public string url = @"http://www.quirksmode.org/html5/videos/big_buck_bunny.mp4";
    public Renderer quad;
    private Texture2D texture;
    private int width, height;
    private byte[] gconvertedFrameBuffer;
    private bool isNewFrameReady = false;

    // Use this for initialization
    void Start()
    {
#if !UNITY_EDITOR && UNITY_METRO
        Task t = Task.Factory.StartNew(StartVideo);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (isNewFrameReady)
        {
            if (texture == null)
            {
                //Debug.Log("create texture: width:" + width + "height:" + height);
                texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                quad.material.mainTexture = texture;
            }
            //Debug.Log("apply texture");
            texture.LoadRawTextureData(gconvertedFrameBuffer);
            texture.Apply();
        }
    }

    void StartVideo()
    {
        //Debug.Log("enable");
        ffmpeg.av_register_all();
        ffmpeg.avcodec_register_all();
        ffmpeg.avformat_network_init();


        UnityEngine.Debug.Log("url:"+ url);

        var pFormatContext = ffmpeg.avformat_alloc_context();
        if (ffmpeg.avformat_open_input(&pFormatContext, url, null, null) != 0)
            throw new ApplicationException(@"Could not open file");

        if (ffmpeg.avformat_find_stream_info(pFormatContext, null) != 0)
            throw new ApplicationException(@"Could not find stream info");
        AVStream* pStream = null;
        for (var i = 0; i < pFormatContext->nb_streams; i++)
            if (pFormatContext->streams[i]->codec->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                pStream = pFormatContext->streams[i];
                break;
            }
        if (pStream == null)
            throw new ApplicationException(@"Could not found video stream");

        var codecContext = *pStream->codec;

        width = codecContext.width;
        height = codecContext.height;
        var sourcePixFmt = codecContext.pix_fmt;
        var codecId = codecContext.codec_id;
        var destinationPixFmt = AVPixelFormat.AV_PIX_FMT_RGB24;
        var pConvertContext = ffmpeg.sws_getContext(width, height, sourcePixFmt,
            width, height, destinationPixFmt,
            ffmpeg.SWS_FAST_BILINEAR, null, null, null);
        if (pConvertContext == null)
            throw new ApplicationException(@"Could not initialize the conversion context");

        var pConvertedFrame = ffmpeg.av_frame_alloc();
        var convertedFrameBufferSize = ffmpeg.av_image_get_buffer_size(destinationPixFmt, width, height, 1);
        var convertedFrameBuffer = new byte[convertedFrameBufferSize];
        byte* a;
        fixed (byte* pSource = convertedFrameBuffer)
        {
            a = pSource;
        }
        gconvertedFrameBuffer = convertedFrameBuffer;

        var dstData = new byte_ptrArray4();
        var dstLinesize = new int_array4();
        ffmpeg.av_image_fill_arrays(ref dstData, ref dstLinesize, a, destinationPixFmt, width, height, 1);

        var pCodec = ffmpeg.avcodec_find_decoder(codecId);
        if (pCodec == null)
            throw new ApplicationException(@"Unsupported codec");

        // reusing codec context from stream info, initally it was looking like this: 
        // AVCodecContext* pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec); // but this is not working for all kind of codecs
        var pCodecContext = &codecContext;
        if ((pCodec->capabilities & ffmpeg.AV_CODEC_CAP_TRUNCATED) == ffmpeg.AV_CODEC_CAP_TRUNCATED)
            pCodecContext->flags |= ffmpeg.AV_CODEC_FLAG_TRUNCATED;

        if (ffmpeg.avcodec_open2(pCodecContext, pCodec, null) < 0)
            throw new ApplicationException(@"Could not open codec");

        var pDecodedFrame = ffmpeg.av_frame_alloc();

        AVPacket packet = new AVPacket();
        ffmpeg.av_init_packet(&packet);

        isNewFrameReady = true;
        Boolean finished = false;
        //Stopwatch stopwatch = new Stopwatch();
        //int count = 0;
        while (!finished)
        {
            //stopwatch.Reset();
            //stopwatch.Start();
            var pPacket = &packet;
            if (ffmpeg.av_read_frame(pFormatContext, pPacket) < 0)
                throw new ApplicationException(@"Could not read frame");
            //stopwatch.Stop();
            //UnityEngine.Debug.Log("av_read_frame time:" + stopwatch.ElapsedMilliseconds);

            if (pPacket->stream_index != pStream->index)
                continue;
            
            //stopwatch.Reset();
            //stopwatch.Start();
            if (ffmpeg.avcodec_send_packet(pCodecContext, pPacket) < 0)
                throw new ApplicationException("Error while sending packet");
            if (ffmpeg.avcodec_receive_frame(pCodecContext, pDecodedFrame) < 0)
                continue;
            //stopwatch.Stop();
            //UnityEngine.Debug.Log("decode time:" + stopwatch.ElapsedMilliseconds);

            //stopwatch.Reset();
            //stopwatch.Start();
            ffmpeg.sws_scale(pConvertContext, pDecodedFrame->data, pDecodedFrame->linesize, 0, height, dstData, dstLinesize);
            //stopwatch.Stop();
            //UnityEngine.Debug.Log("sws_scale time:" + stopwatch.ElapsedMilliseconds);

            ffmpeg.av_packet_unref(pPacket);
            ffmpeg.av_frame_unref(pDecodedFrame);
            //count++;
            //if(count == 5)
            //    finished = true;
        }
        ffmpeg.av_free(pConvertedFrame);
        ffmpeg.sws_freeContext(pConvertContext);
        ffmpeg.av_free(pDecodedFrame);
        ffmpeg.avcodec_close(pCodecContext);
        ffmpeg.avformat_close_input(&pFormatContext);
    }
}
