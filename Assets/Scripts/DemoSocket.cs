using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Linq;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine.UI;
using HoloLensXboxController;
using System.Collections;
#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
using MyUDP;
using Windows.Devices.Power;
#endif

public class DemoSocket : MonoBehaviour
{
    public string port;
    public string externalIP;
    public string externalPort;
    private string local;
    private string leftStickValue = "";
    // Modify 4/14/2017
    private ControllerInput controllerInput;
    private Text gps;
    //Modify 4/13/2017
    private Text[] texts;
    private GameObject[] infos;
    private string[] originalTexts;
    //Modify 4/13/2017
    private readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();
    private GameObject [] parents;
    private bool controllerConnection = false;
    private MapController map;
    private VideoPanelManager3 videoManager;
    private GameObject anchor;
    private Transform anchorTransform;
    private Vector3[] vectors;
    private bool prepared = false;
    private PositionInfo position;
    private bool good = true;
    public ControllerInput ControllerInput
    {
        get
        {
            return controllerInput;
        }

        set
        {
            controllerInput = value;
        }
    }

    public bool Prepared
    {
        get
        {
            return prepared;
        }

        set
        {
            prepared = value;
        }
    }

    public Vector3[] Vectors
    {
        get
        {
            return vectors;
        }

        set
        {
            vectors = value;
        }
    }

    public bool Good
    {
        get
        {
            return good;
        }

        set
        {
            good = value;
        }
    }

#if UNITY_UWP
    DatagramSocket socket;

    async void Start()
    {
        gps = GameObject.Find("Canvas/PanelMap/PositionText").GetComponent<Text>();
        Debug.Log("Local Ip At Start "+local);
        local = externalIP;

        controllerInput = new ControllerInput(0, 0.19f);

        Debug.Log("Waiting for a connection...");

        infos = GameObject.FindGameObjectsWithTag("label");

        parents = GameObject.FindGameObjectsWithTag("parent");
        map = MapController.GetMapController();
        videoManager = GameObject.Find("Canvas/PanelVideo").GetComponent<VideoPanelManager3>();
        texts = new Text[infos.Length];
        originalTexts = new string[infos.Length];
        anchor = GameObject.Find("Canvas/PanelVideo/anchor");
        anchorTransform = anchor.transform;
        for (int i = 0;infos!=null&&i<infos.Length;i++) {
            texts[i] = infos[i].GetComponent<Text>();
            if (texts[i]!=null) {
                originalTexts[i] = texts[i].text;
            }
        }
        //Modify 4/13/2017
        socket = new DatagramSocket();
        socket.MessageReceived += Socket_MessageReceived;

        HostName IP = null;
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            IP = Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
            .SingleOrDefault(
                hn =>
                    hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                    == icp.NetworkAdapter.NetworkAdapterId);

            await socket.BindEndpointAsync(IP, port);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }

        var message = "hello from " + IP;
        await SendMessage(message);
        await SendMessage("hello");
        
        Debug.Log("exit start");
    }

    private async void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
            Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        //Read the message that was received from the UDP echo client.
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        Byte[] header = new Byte[2];

        await streamIn.ReadAsync(header, 0, 2);

        string hearderStr = BitConverter.ToString(header);
        // 遥测数据头 AA 55
        Debug.Log("header:" + hearderStr);
        // 验证同步码
        if ("AA-55" == hearderStr)
        {
            Byte[] data = new Byte[80];
            await streamIn.ReadAsync(data, 0, 80);
            // 类型为数据
            DroneData droneData = new DroneData(data);

            int sum = 0;
            for (int i = 0; i < 80; i++)
            {
                sum += data[i];
            }
            string str = Convert.ToString(sum, 2);
            string real = str;
            if (real.Length >= 8)
            {
                real = str.Substring(str.Length - 8);
            }
            下面的代码是校验和对比，测试时可以注释掉
            Byte verification = Convert.ToByte(real, 2);
            if (verification != droneData.Verification)
            {
                Debug.Log("Verification byte is wrong");
                return;
            }

            Debug.Log("Data: "+BitConverter.ToString(data));
            if (anchor!=null&&videoManager.Captured) {
                float leftTopX = Convert.ToSingle(droneData.LeftTopX);
                float leftTopY = Convert.ToSingle(droneData.LeftTopY);
                float rightBottomX = Convert.ToSingle(droneData.RightBottomX);
                float rightBottomY = Convert.ToSingle(droneData.RightBottomY);

                if (leftTopX < 0) {
                    leftTopX = 0;
                } else if (leftTopX > 1920) {
                    leftTopX = 1920;
                }
                if (leftTopY < 0) {
                    leftTopY = 0;
                } else if (leftTopY > 1080) {
                    leftTopY = 1080;
                }
                if (rightBottomX < 0)
                {
                    rightBottomX = 0;
                }
                else if (rightBottomX > 1920)
                {
                    rightBottomX = 1920;
                }
                if (rightBottomY < 0)
                {
                    rightBottomY = 0;
                }
                else if (rightBottomY > 1080)
                {
                    rightBottomY = 1080;
                }
                vectors = new Vector3[2];
                vectors[0] = new Vector3(leftTopX,-leftTopY,0);
                vectors[1] = new Vector3(rightBottomX, -rightBottomY, 0);
                prepared = true;
            }  

            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {

                    for (int i = 0; i < infos.Length; i++)
                    {

                        //Modify 4/13/2017
                        string name = infos[i].name;
                        switch (name)
                        {
                            case "time":
                                int year = Convert.ToInt16(droneData.Fly.Year);
                                int min = Convert.ToInt16(droneData.Fly.Min);
                                int month = Convert.ToInt16(droneData.Fly.Month);
                                int date = Convert.ToInt16(droneData.Fly.Date);
                                int hour = Convert.ToInt16(droneData.Fly.Hour);
                                int sec = Convert.ToInt16(droneData.Fly.Sec);
                                int percentSec = Convert.ToInt16(droneData.Fly.PercentSec);
                                if (year < 0 || year > 99)
                                {
                                    year = 0;
                                }
                                if (month < 0 || month > 12)
                                {
                                    month = 0;
                                }
                                if (date < 0 || date > 31)
                                {
                                    date = 0;
                                }
                                if (hour < 0 || hour > 23)
                                {
                                    hour = 0;
                                }
                                if (min < 0 || min > 59)
                                {
                                    min = 0;
                                }
                                if (sec < 0 || sec > 59)
                                {
                                    sec = 0;
                                }
                                if (percentSec < 0 || percentSec > 99)
                                {
                                    percentSec = 0;
                                }
                                texts[i].text = originalTexts[i] + (" " + year + "年 " + month + "月 " + date + "日 " + hour + "时 " + min + "分 " + sec + "秒 " + percentSec + "百分秒").ToString();
                                break;
                            case "flytime":
                                int v8 = droneData.Fly.Stars;
                                if (v8 < 0) {
                                    v8 = 0;
                                } else if (v8>24) {
                                    v8 = 24;
                                }
                                texts[i].text = originalTexts[i] + (v8).ToString() + "颗";
                                break;
                            case "flyangle2":
                                int v5 = (int)(droneData.Fly.MovingAngle * 0.01);
                                if (v5 < 0) {
                                    v5 = 0;
                                } else if (v5>360) {
                                    v5 = 360;
                                }
                                texts[i].text = originalTexts[i] + (v5).ToString() + "°";
                                break;
                            case "updownangle2":
                                int v6 = (int)(droneData.Fly.UpdownAngle * 0.01);
                                if (v6 < -90)
                                {
                                    v6 = -90;
                                }
                                else if (v6 > 90)
                                {
                                    v6 = 90;
                                }
                                texts[i].text = originalTexts[i] + (v6).ToString() + "°";
                                break;
                            case "rotateangle":
                                int v7 = (int)(droneData.Fly.UpdownAngle * 0.01);
                                if (v7 < -90)
                                {
                                    v7 = 90;
                                }
                                else if (v7 > 90)
                                {
                                    v7 = 90;
                                }
                                texts[i].text = originalTexts[i] + (droneData.Fly.RotateAngle * 0.01f).ToString() + "°";
                                break;
                            case "attitude":
                                int v9 = droneData.Fly.Height;
                                if (v9 < -500) {
                                    v9 = -500;
                                } else if (v9>6000) {
                                    v9 = 6000;
                                }
                                texts[i].text = originalTexts[i] + (v9).ToString() + "M";
                                break;
                            case "speed":
                                int v10 = droneData.Fly.Speed;
                                if (v10 < 0) {
                                    v10 = 0;
                                } else if (v10 > 1200) {
                                    v10 = 1200;
                                }
                                texts[i].text = originalTexts[i] + (v10).ToString() + "Km\\h";
                                break;
                            case "longitude":
                                float f = droneData.Fly.Longtitude * 0.0000001f;

                                if (f < -180)
                                {
                                    f = -180;
                                }
                                else if (f > 180)
                                {
                                    f = 180;
                                }
                                texts[i].text = originalTexts[i] + (f).ToString() + "°";
                                //经度
                                position = new PositionInfo();
                                position.Longti = f;
                                gps.text = texts[i].text;
                                break;
                            case "latitude":
                                //纬度

                                float f2 = droneData.Fly.Latitude * 0.0000001f;

                                if (f2 < -90)
                                {
                                    f2 = -90;
                                }
                                else if (f2 > 90)
                                {
                                    f2 = 90;
                                }

                                texts[i].text = originalTexts[i] + (f2).ToString() + "°";
                                gps.text += texts[i].text;
                                position.Lati = f2;
                                //下面这行代码会在地图上画点，但是之前做的都不符合要求，现在被我注释掉了
                                MapController.GetMapController().AddPoints(position.Longti,position.Lati);                     

                                break;
                            case "status":
                                string state = Convert.ToString(droneData.State2, 2).PadLeft(8, '0');
                                string s = "";
                                if (state[0] == '0') {
                                    s = "未跟踪好";
                                    good = false;
                                } else if (state[0]=='1') {
                                    s = "已跟踪好";
                                    good = true;
                                }
                                texts[i].text = originalTexts[i] + s;
                                break;
                            case "hangji":
                                int v11 = (int)(droneData.Fly.TrackAngle * 0.1);
                                if (v11 < 0) {
                                    v11 = 0;
                                } else if (v11>360) {
                                    v11 = 360;
                                }
                                texts[i].text = originalTexts[i] + (v11).ToString() + "°";
                                break;
                            case "flyangle":
                                int value1 = (int)(droneData.PtUpDown * 0.01);
                                if (value1 > 90)
                                {
                                    value1 = 90;
                                }
                                else if (value1 < -90)
                                {
                                    value1 = -90;
                                }
                                texts[i].text = originalTexts[i] + value1.ToString()+ "°";
                                break;
                            case "updownangle":
                                int value = (int)(droneData.PtUpDown*0.01);
                                if (value > 2) {
                                    value = 2;
                                } else if (value<-90) {
                                    value = -90;
                                }
                                texts[i].text = originalTexts[i] + value.ToString()+ "°";
                                break;
                            case "gyro":
                                Byte mode = droneData.WorkMode;
                                string str1 = "";
                                switch (mode) {
                                    case 0x20:
                                        str1 = "目标指向";
                                        break;
                                    case 0x40:
                                        str1 = "手动搜索";
                                        break;
                                    case 0x80:
                                        str1 = "目标跟踪";
                                        break;
                                }
                                texts[i].text = originalTexts[i] + str1;
                                break;
                            case "locationdrive":
                                int v1 = droneData.UpDownOffset;
                                if (v1 < -540) {
                                    v1 = -540;
                                } else if (v1>540) {
                                    v1 = 540;
                                }
                                texts[i].text = originalTexts[i] + v1.ToString()+"像素";
                                break;
                            case "directanglespeed":
                                int v3 = droneData.DirectAngle;
                                v3 = (int)(v3 * 0.01);
                                texts[i].text = originalTexts[i] + v3.ToString()+ "°/s";
                                break;
                            case "updownanglespeed":
                                int v4 = droneData.UpDownAngle;
                                v4 = (int)(v4 * 0.01);
                                texts[i].text = originalTexts[i] + v4.ToString() + "°/s";
                                break;
                            case "updowndrive":
                                int v2 = droneData.DirectOffset;
                                if (v2 < -960) {
                                    v2 = -960;
                                } else if (v2>960) {
                                    v2 = 960;
                                }
                                texts[i].text = originalTexts[i] + v2.ToString()+"像素";
                                break;
                            case "ptzstatus":
                                UInt32 seq = droneData.Sequence;
                                if (seq < 0x00000000) {
                                    seq = 0x00000000;
                                } else if (seq>0xffffffff) {
                                    seq = 0xffffffff;
                                }
                                texts[i].text = originalTexts[i] + seq.ToString();
                                break;
                            case "battery":
                                var aggBattery = Battery.AggregateBattery;
                                var report = aggBattery.GetReport();
                                double full = Convert.ToDouble(report.FullChargeCapacityInMilliwattHours.ToString());
                                double cur = Convert.ToDouble(report.RemainingCapacityInMilliwattHours.ToString());
                                double percent = (cur / full) * 100;
                                texts[i].text = originalTexts[i] + (int)percent + "%";
                                break;
                            case "wifistatus":
                                string wifi = (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == true ? "已连接" : "未连接");
                                texts[i].text = originalTexts[i] + wifi;
                                break;
                            case "controllerstatus":
                                string controllerStatus = (controllerConnection == true ? "手柄已连接" : "手柄未连接");
                                texts[i].text = originalTexts[i] + controllerStatus;
                                break;
                            case "rheight":
                                float v13 = droneData.Fly.RHeight * 0.1f;
                                if (v13 < -1000) {
                                    v13 = -1000f;
                                } else if (v13>3000) {
                                    v13 = 3000f;
                                }
                                texts[i].text = originalTexts[i] + v13 +"m";
                                break;
                        }
                    }
                }, false);

        }

        if (ExecuteOnMainThread.Count == 0)
        {
            ExecuteOnMainThread.Enqueue(() =>
            {
                    //Thermostat.Temperature = float.Parse(message);
            });
        }
    }

    private async System.Threading.Tasks.Task SendMessage(string message)
    {
        using (var stream = await socket.GetOutputStreamAsync(new Windows.Networking.HostName(externalIP), externalPort))
        {
            using (var writer = new Windows.Storage.Streams.DataWriter(stream))
            {
                var data = Encoding.UTF8.GetBytes(message);
                writer.WriteBytes(data);
                await writer.StoreAsync();
                //Modify 4/14/2017
                await writer.FlushAsync();
                //Modify 4/14/2017
                Debug.Log("Sent: " + message);
            }
        }
    }

    private async System.Threading.Tasks.Task SendMessage(byte[] data)
    {
        using (var stream = await socket.GetOutputStreamAsync(new Windows.Networking.HostName(externalIP), externalPort))
        {
            using (var writer = new Windows.Storage.Streams.DataWriter(stream))
            {
                writer.WriteBytes(data);
                await writer.StoreAsync();
                await writer.FlushAsync();
            }
        }
    }
    //发送选框的坐标指令
    public async void SendCoordinateInfo(float x1,float y1,float x2,float y2)
    {

        if (x1 < 80)
        {
            x1 = 80;
        }
        else if (x1 > 1839)
        {
            x1 = 1839;
        }
        if (y1 < 80)
        {
            y1 = 80;
        }
        else if (y1 > 999)
        {
            y1 = 999;
        }
        if (x2 < 80)
        {
            x2 = 80;
        }
        else if (x2 > 1839)
        {
            x2 = 1839;
        }
        if (y2 < 80)
        {
            y2 = 80;
        }
        else if (y2 > 999)
        {
            y2 = 999;
        }

        byte[] data = new byte[45];
        data[0] = 0xEB;
        data[1] = 0x90;
        data[2] = 0xB3;
        UInt16 xv1 = Convert.ToUInt16(x1);
        UInt16 yv1 = Convert.ToUInt16(y1);

        byte[] xvArray = BitConverter.GetBytes(xv1);
        byte[] yvArray = BitConverter.GetBytes(yv1);
        data[3] = xvArray[0];
        data[4] = xvArray[1];

        data[5] = yvArray[0];
        data[6] = yvArray[1];
        UInt16 xv2 = Convert.ToUInt16(x2);
        UInt16 yv2 = Convert.ToUInt16(y2);
        byte[] xvArray2 = BitConverter.GetBytes(xv2);
        byte[] yvArray2 = BitConverter.GetBytes(yv2);
        data[7] = xvArray2[0];
        data[8] = xvArray2[1];
        data[9] = yvArray2[0];
        data[10] = yvArray2[1];
        int sum = 0;
        for (int i = 2;i<=43;i++) {
            sum += data[i];
        }
        string str = Convert.ToString(sum, 2);
        string real = str.Substring(str.Length-8);
        data[44] = Convert.ToByte(real,2);//???
        await SendMessage(data);
    }

    private bool searchDone = false;
    //发送目标搜索周期性指令
    public async void SendSearchManually(float x1, float y1)
    {

        if (x1 < -100)
        {
            x1 = -100;
        }
        else if (x1 > 100)
        {
            x1 = 100;
        }
        if (y1 < -100)
        {
            y1 = -100;
        }
        else if (y1 > 100)
        {
            y1 = 100;
        }

        byte[] data = new byte[45];
        data[0] = 0xEB;
        data[1] = 0x90;
        data[2] = 0x18;

        Int16 xv1 = Convert.ToInt16(x1);
        Int16 yv1 = Convert.ToInt16(y1);

        byte[] xvArray = BitConverter.GetBytes(xv1);
        byte[] yvArray = BitConverter.GetBytes(yv1);
        data[3] = xvArray[0];
        data[4] = xvArray[1];

        data[5] = yvArray[0];
        data[6] = yvArray[1];

        data[7] = 0x00;
        data[8] = 0x00;
        data[9] = 0x00;
        data[10] = 0x00;

        int sum = 0;
        for (int i = 2; i <= 43; i++)
        {
            sum += data[i];
        }
        string str = Convert.ToString(sum, 2);
        string real = str;
        if (real.Length >= 8)
        {
            real = str.Substring(str.Length - 8);
        }

        data[44] = Convert.ToByte(real, 2);//???

        await SendMessage(data);
    }
    //发送悬停指令
    public async void SendHoverOrder()
    {
        byte[] data = new byte[45];
        data[0] = 0xEB;
        data[1] = 0x90;
        data[2] = 0xC3;
        data[3] = 0x00;
        data[4] = 0x00;

        data[5] = 0x00;
        data[6] = 0x00;

        data[7] = 0x00;
        data[8] = 0x00;
        data[9] = 0x00;
        data[10] = 0x00;
        int sum = 0;
        for (int i = 2; i <= 43; i++)
        {
            sum += data[i];
        }
        string str = Convert.ToString(sum, 2);
        string real = str;
        if (real.Length >= 8)
        {
            real = str.Substring(str.Length - 8);
        }
        data[44] = Convert.ToByte(real, 2);//???
        await SendMessage(data);
    }
    //发送半自动指令
    public async void SendHalfAutoOrder()
    {
        byte[] data = new byte[45];
        data[0] = 0xEB;
        data[1] = 0x90;
        data[2] = 0xC4;
        data[3] = 0x00;
        data[4] = 0x00;

        data[5] = 0x00;
        data[6] = 0x00;

        data[7] = 0x00;
        data[8] = 0x00;
        data[9] = 0x00;
        data[10] = 0x00;
        int sum = 0;
        for (int i = 2; i <= 43; i++)
        {
            sum += data[i];
        }
        string str = Convert.ToString(sum, 2);
        string real = str;
        if (real.Length >= 8)
        {
            real = str.Substring(str.Length - 8);
        }
        data[44] = Convert.ToByte(real, 2);//???
        await SendMessage(data);
    }
    //发送全自动指令
    public async void SendCompleteAutoOrder()
    {
        byte[] data = new byte[45];
        data[0] = 0xEB;
        data[1] = 0x90;
        data[2] = 0xC5;
        data[3] = 0x00;
        data[4] = 0x00;

        data[5] = 0x00;
        data[6] = 0x00;

        data[7] = 0x00;
        data[8] = 0x00;
        data[9] = 0x00;
        data[10] = 0x00;
        int sum = 0;
        for (int i = 2; i <= 43; i++)
        {
            sum += data[i];
        }
        string str = Convert.ToString(sum, 2);
        string real = str;
        if (real.Length >= 8)
        {
            real = str.Substring(str.Length - 8);
        }
        data[44] = Convert.ToByte(real, 2);//???
        await SendMessage(data);
    }
    //手柄按钮被按以后发送信息
    private async System.Threading.Tasks.Task SendControllerInfo(string message)
    {
        using (var stream = await socket.GetOutputStreamAsync(new Windows.Networking.HostName(local), externalPort))
        {
            using (var writer = new Windows.Storage.Streams.DataWriter(stream))
            {
                var data = Encoding.UTF8.GetBytes(message);
                writer.WriteBytes(data);
                await writer.StoreAsync();

                await writer.FlushAsync();
                Debug.Log("Sent: " + message);
            }
        }
    }
    // Modify 4/14/2017 手柄按钮被按以后发送信息
    private async void GetControllerInfo() {
        leftStickValue = "AGetPressed";
        await SendControllerInfo(leftStickValue);
    }

    // Modify 4/14/2017
#else
    void Start()
    {

    }
#endif
    // Modify 4/14/2017
#if UNITY_UWP
    // Update is called once per frame
    void Update()
    {

        try
        {
            controllerInput.Update();
            controllerConnection = true;
        }
        catch (ArgumentOutOfRangeException e)
        {
            controllerConnection = false;
        }

        if (controllerInput.GetButton(ControllerButton.A))
        {
            GetControllerInfo();
        }

        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }

#else
    void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }
#endif
    // Modify 4/14/2017
}
internal class PositionInfo {
    private float longti;
    private float lati;

    public PositionInfo() { }

    public PositionInfo(float longti, float lati)
    {
        this.longti = longti;
        this.lati = lati;
    }

    public float Longti
    {
        get
        {
            return longti;
        }

        set
        {
            longti = value;
        }
    }

    public float Lati
    {
        get
        {
            return lati;
        }

        set
        {
            lati = value;
        }
    }
}