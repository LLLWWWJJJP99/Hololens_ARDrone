using UnityEngine;
using System;
#if !UNITY_EDITOR && UNITY_METRO
using System.Threading.Tasks;
using FFmpeg.AutoGen;
#endif

public unsafe class VideoDecoder2 : MonoBehaviour
{
    // var url = @"";
    public string url = @"http://www.quirksmode.org/html5/videos/big_buck_bunny.mp4";
    public Renderer quad;
    private Texture2D texture;
    private int width, height;
    private byte[] imageFrameBuffer;
    private bool isNewFrameReady = false;
    Boolean finished = false;

    // Use this for initialization
    void Start()
    {
        //StartVideo();
#if !UNITY_EDITOR && UNITY_METRO
        Task t = Task.Factory.StartNew(StartVideo);
#endif
    }

    void OnApplicationQuit()
    {
        finished = true;
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
            texture.LoadRawTextureData(imageFrameBuffer);
            texture.Apply();
        }
    }

#if !UNITY_EDITOR && UNITY_METRO
    void StartVideo()
    {
        url = "udp://192.168.1.102:6666?buffer_size=10000000&fifo_size=400000&overrun_nonfatal=1";
        //url = "tcp://10.0.0.13:6666";
        ffmpeg.av_register_all();
        ffmpeg.avcodec_register_all();
        ffmpeg.avformat_network_init();
        Debug.Log("url:" + url);

        //AVHWAccel* hwaccel = null;
        //hwaccel = ffmpeg.av_hwaccel_next(hwaccel);
        //while (hwaccel != null)
        //{
        //    Debug.Log(hwaccel->pix_fmt);
        //    hwaccel = ffmpeg.av_hwaccel_next(hwaccel);
        //}

        //ffmpeg.avio_alloc_context();
        var pFormatContext = ffmpeg.avformat_alloc_context();
        if (ffmpeg.avformat_open_input(&pFormatContext, url, null, null) != 0)
            return;
        if (ffmpeg.avformat_find_stream_info(pFormatContext, null) != 0)
            return;
        AVStream* pStream = null;
        for (var i = 0; i < pFormatContext->nb_streams; i++)
            if (pFormatContext->streams[i]->codec->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                pStream = pFormatContext->streams[i];
                break;
            }
        if (pStream == null)
            return;
        //throw new ApplicationException(@"Could not found video stream");

        var codecContext = *pStream->codec;
        width = codecContext.width;
        height = codecContext.height;
        var sourcePixFmt = codecContext.pix_fmt;
        var codecId = codecContext.codec_id;
        var destinationPixFmt = AVPixelFormat.AV_PIX_FMT_RGB24;
        var pConvertContext = ffmpeg.sws_getContext(width, height, sourcePixFmt, width, height, destinationPixFmt, ffmpeg.SWS_POINT, null, null, null);
        if (pConvertContext == null)
            throw new ApplicationException(@"Could not initialize the conversion context");

        //var pConvertedFrame = ffmpeg.av_frame_alloc();
        var convertedFrameBufferSize = ffmpeg.av_image_get_buffer_size(destinationPixFmt, width, height, 1);
        //var convertedFrameBuffer = new byte[convertedFrameBufferSize];
        imageFrameBuffer = new byte[convertedFrameBufferSize];
        var dstData = new byte_ptrArray4();
        var dstLinesize = new int_array4();
        fixed (byte* pSource = imageFrameBuffer)
            ffmpeg.av_image_fill_arrays(ref dstData, ref dstLinesize, pSource, destinationPixFmt, width, height, 1);


        AVCodec* pCodec = null;
        ffmpeg.av_find_best_stream(pFormatContext, 0, -1, -1, &pCodec, 0);
        //Debug.Log(pCodec->id);
        var pCodecContext = &codecContext;
        pCodecContext->thread_count = 2;
        //pCodecContext->hwaccel = hwaccel;
        //pCodecContext->pix_fmt = AVPixelFormat.AV_PIX_FMT_D3D11VA_VLD;

        //if ((pCodec->capabilities & ffmpeg.AV_CODEC_CAP_TRUNCATED) == ffmpeg.AV_CODEC_CAP_TRUNCATED)
        //    pCodecContext->flags |= ffmpeg.AV_CODEC_FLAG_TRUNCATED;

        if (ffmpeg.avcodec_open2(pCodecContext, pCodec, null) < 0)
            throw new ApplicationException(@"Could not open codec");
        var pDecodedFrame = ffmpeg.av_frame_alloc();
        var pPacket = ffmpeg.av_packet_alloc();


        //AVHWAccel* hwaccel = null;
        //pCodecContext->pix_fmt = AVPixelFormat.AV_PIX_FMT_D3D11VA_VLD;
        //while ((hwaccel = ffmpeg.av_hwaccel_next(hwaccel)) != null)
        //{
        //    if (hwaccel->id == pCodecContext->codec_id && hwaccel->pix_fmt == pCodecContext->pix_fmt)
        //    {
        //        pCodecContext->hwaccel = hwaccel;
        //        Debug.Log("codec:" + hwaccel->id + "  pixfmt:" + hwaccel->pix_fmt);
        //        break;
        //    }
        //}
        isNewFrameReady = true;
        while (!finished && ffmpeg.av_read_frame(pFormatContext, pPacket) >= 0)
        {
            //try
            //{
                if (pPacket->stream_index != pStream->index)
                    continue;
                //DateTime startTime = DateTime.Now;
                if (ffmpeg.avcodec_send_packet(pCodecContext, pPacket) < 0)
                    throw new ApplicationException("Error while sending packet");
                int ret = 0;
                while (ret >= 0)
                {
                    ret = ffmpeg.avcodec_receive_frame(pCodecContext, pDecodedFrame);
                    if (ret == 0)
                    {
                        ffmpeg.sws_scale(pConvertContext, pDecodedFrame->data, pDecodedFrame->linesize, 0, height, dstData, dstLinesize);
                    }
                    else
                    {
                        break;
                    }
                }
            //}
            //finally
            //{
            //    ffmpeg.av_packet_unref(pPacket);
            //    ffmpeg.av_frame_unref(pDecodedFrame);
            //}
        }
        //ffmpeg.av_free(pConvertedFrame);
        ffmpeg.sws_freeContext(pConvertContext);
        ffmpeg.av_free(pDecodedFrame);
        ffmpeg.avcodec_close(pCodecContext);
        ffmpeg.avformat_close_input(&pFormatContext);
    }
#endif
}