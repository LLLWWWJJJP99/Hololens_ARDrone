using UnityEngine;
using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using Windows.Storage;
using Windows.Storage.Streams;
#endif

public class SaveLoadFile : MonoBehaviour {
    public Renderer quad;
    private Byte[] imageFrameBuffer;
    private Boolean isReady = false;
    private string[] fileNames = new string[4];
    private int count = 0;
    private Dictionary<string, Texture2D> dict = new Dictionary<string, Texture2D>();
    //define filePath
    private MapController controller;
    private static SaveLoadFile self;

    public Dictionary<string, Texture2D> Dict
    {
        get
        {
            return dict;
        }

        set
        {
            dict = value;
        }
    }

    void Awake()
    {
        self = this;
        fileNames[0] = "L12.jpg";
        fileNames[1] = "L15.jpg";
        fileNames[2] = "L17.jpg";
        fileNames[3] = "L19.jpg";
    }

    void Start()
    {
        controller = MapController.GetMapController();
    }

    void Update()
    {
        //if (texture == null && isReady)
        //{
        //    texture = new Texture2D(512, 256, TextureFormat.RGB24, false);
        //    quad.material.mainTexture = texture;
        //    texture.LoadImage(imageFrameBuffer);
        //    texture.Apply();
        //}
    }

    public static SaveLoadFile GetSaveLoadFile() {
        return self;
    }

//    public void IncreaseResolution() {
//#if WINDOWS_UWP
//        MapController.GetMapController().IncreaseResolution();
//        texture = null;
//        count++;
//        if (count==4) {
//            count = 3;
//        }
//        ReadData();
//        for (int i = 0; i < controller.Dots.Count; i++)
//        {
//            controller.MovePointsToRealPosition(controller.Coordinates[i].x, controller.Coordinates[i].y,i);
//        }
//#endif
//    }

//    public void DecreaseResolution() {
//#if WINDOWS_UWP
//        MapController.GetMapController().DecreaseResolution();
//        texture = null;
//        count--;
//        if (count==-1) {
//            count = 0;
//        }
//        ReadData();
//        for (int i = 0; i < controller.Dots.Count; i++)
//        {
//            controller.MovePointsToRealPosition(controller.Coordinates[i].x, controller.Coordinates[i].y,i);
//        }
//#endif
//    }
#if WINDOWS_UWP

    public async void ReadData()
    {
        StorageFolder localFolder = KnownFolders.CameraRoll;
        Debug.Log("start load");
        StorageFile file = await localFolder.GetFileAsync(fileNames[count].Trim());
        using (var inputStream = await file.OpenAsync(FileAccessMode.Read))
        {
            var reader = new DataReader(inputStream.GetInputStreamAt(0));
            imageFrameBuffer = new byte[inputStream.Size];
            await reader.LoadAsync((uint)inputStream.Size);
            reader.ReadBytes(imageFrameBuffer);
            Texture2D texture = new Texture2D(512, 256, TextureFormat.RGB24, false);
            texture.LoadImage(imageFrameBuffer);
            dict.Add(fileNames[count],texture);
        }
        isReady = true;
    }
#endif
}
