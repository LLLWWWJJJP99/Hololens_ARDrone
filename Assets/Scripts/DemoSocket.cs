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
    private GoogleMap map;
    private VideoPanelManager3 videoManager;
    private GameObject anchor;
    private Transform anchorTransform;
    private Vector3[] vectors;
    private bool prepared = false;
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
        map = GameObject.Find("Canvas/PanelMap/Plane").GetComponent<GoogleMap>();
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
                vectors[0] = new Vector3(leftTopX,leftTopY,0);
                vectors[1] = new Vector3(rightBottomX, rightBottomY, 0);
                prepared = true;
                
            }  
            Debug.Log("data:" + droneData.Fly.Year+" || "+droneData.LeftTopX);
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    string state0 = Convert.ToString(droneData.State0, 2).PadLeft(8, '0'); ;

                    char[] s0 = state0.ToCharArray();

                    string state1 = Convert.ToString(droneData.State1, 2).PadLeft(8, '0'); ;

                    char[] s1 = state0.ToCharArray();

                    State0Object object0 = new State0Object();
                    if (s0[0] == '0')
                    {
                        object0.DirectionDrive = "空闲";
                    }
                    else if (s0[0] == '1')
                    {
                        object0.DirectionDrive = "使能";
                    }

                    if (s0[1] == '0')
                    {
                        object0.UpDownDrive = "空闲";
                    }
                    else if (s0[1] == '1')
                    {
                        object0.UpDownDrive = "使能";
                    }

                    if (s0[5] == '0')
                    {
                        object0.Light = "正常图";
                    }
                    else if (s0[5] == '1')
                    {
                        object0.Light = "测试图";
                    }

                    State1Object object1 = new State1Object();
                    if (s1[0] == '0' && s1[1] == '0' && s1[2] == '0')
                    {
                        object1.ImageEnhancement = "不增强";
                    }
                    else if (s1[0] == '1' && s1[1] == '0' && s1[2] == '0')
                    {
                        object1.ImageEnhancement = "弱增强";
                    }
                    else if (s1[0] == '0' && s1[1] == '1' && s1[2] == '0')
                    {
                        object1.ImageEnhancement = "中增强";
                    }
                    else if (s1[0] == '1' && s1[1] == '1' && s1[2] == '0')
                    {
                        object1.ImageEnhancement = "强增强";
                    }

                    char[] tmp = new char[3];
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        tmp[i] = s1[i + 3];
                    }

                    switch (new string(tmp))
                    {
                        case "000":
                            object1.Multiples = "预留";
                            break;
                        case "001":
                            object1.Multiples = "1倍";
                            break;
                        case "010":
                            object1.Multiples = "2倍";
                            break;
                    }

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
                                if (min < 0 || min > 99)
                                {
                                    min = 0;
                                }
                                if (sec < 0 || sec > 99)
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

                                break;
                            case "battery2":

                                break;
                            case "flyangle2":
                                texts[i].text = originalTexts[i] + (droneData.Fly.MovingAngle * 0.01f).ToString() + "°";
                                break;
                            case "updownangle2":
                                texts[i].text = originalTexts[i] + (droneData.Fly.UpdownAngle * 0.01f).ToString() + "°";
                                break;
                            case "rotateangle":
                                texts[i].text = originalTexts[i] + (droneData.Fly.RotateAngle * 0.01f).ToString() + "°";
                                break;
                            case "attitude":
                                texts[i].text = originalTexts[i] + (droneData.Fly.Height).ToString() + "M";
                                break;
                            case "longitude":
                                float f = droneData.Fly.Longtitude;
                                if (f < -90)
                                {
                                    f = -90f;
                                }
                                else if (f > 90)
                                {
                                    f = 90f;
                                }
                                texts[i].text = originalTexts[i] + (f).ToString() + "°";
                                map.Lonti = f; //经度

                                gps.text = texts[i].text;
                                break;
                            case "latitude":
                                float f2 = droneData.Fly.Latitude * 0.0000001f;
                                if (f2 < -180)
                                {
                                    f2 = -180f;
                                }
                                else if (f2 > 180)
                                {
                                    f2 = 180f;
                                }
                                texts[i].text = originalTexts[i] + (f2).ToString() + "°";

                                map.Al = f2; //纬度 刚量少* -10^7?
                                gps.text += texts[i].text;
                                break;
                            case "status":

                                break;
                            case "hangji":
                                texts[i].text = originalTexts[i] + (droneData.Fly.TrackAngle * 0.1f).ToString() + "°";
                                break;
                            case "focallength":
                                //texts[i].text = originalTexts[i] + (droneData.CFocuse * 0.01f).ToString() + "mm";
                                break;
                            case "resolutionh":
                                //texts[i].text = originalTexts[i] + (droneData.Resolution_h).ToString() + "像素";
                                break;
                            case "resolutionv":
                                //texts[i].text = originalTexts[i] + (droneData.Resolution_v).ToString() + "像素";
                                break;
                            case "enforce":
                                texts[i].text = originalTexts[i] + (object1.ImageEnhancement).ToString();
                                break;
                            case "zoomfactor":

                                break;
                            case "status2":

                                break;
                            case "flyangle":

                                break;
                            case "updownangle":

                                break;
                            case "gyro":

                                break;
                            case "locationdrive":
                                texts[i].text = originalTexts[i] + (object0.DirectionDrive).ToString();
                                break;
                            case "updowndrive":
                                texts[i].text = originalTexts[i] + (object0.UpDownDrive).ToString();
                                break;
                            case "ptzstatus":

                                break;
                            case "battery":
                                Debug.Log("battery");
                                var aggBattery = Battery.AggregateBattery;
                                var report = aggBattery.GetReport();
                                double full = Convert.ToDouble(report.FullChargeCapacityInMilliwattHours.ToString());
                                double cur = Convert.ToDouble(report.RemainingCapacityInMilliwattHours.ToString());
                                double percent = (cur / full) * 100;
                                texts[i].text = originalTexts[i] + (int)percent + "%";
                                break;
                            case "wifistatus":
                                Debug.Log("wifistatus");
                                string wifi = (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == true ? "已连接" : "未连接");
                                texts[i].text = originalTexts[i] + wifi;
                                break;
                            case "controllerstatus":
                                Debug.Log("controllerstatus");
                                string controllerStatus = (controllerConnection == true ? "手柄已连接" : "手柄未连接");
                                texts[i].text = originalTexts[i] + controllerStatus;
                                break;
                            case "bomen":

                                break;
                            case "zijian":

                                break;
                            case "genzong":

                                break;
                            case "rengong":

                                break;
                            case "zidong":

                                break;
                            case "photo":

                                break;
                            case "recording":

                                break;
                            case "xunhang":

                                break;
                            case "raofei":

                                break;
                            case "pianchah":

                                break;
                            case "pianchaf":

                                break;
                            case "weitiaoh":

                                break;
                            case "weitiaof":

                                break;
                            case "t1h":

                                break;
                            case "t1z":

                                break;
                            case "t2h":

                                break;
                            case "t2z":

                                break;
                            case "target":

                                break;
                            case "targeth":

                                break;
                            case "targetz":

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
        data[44] = Convert.ToByte(real.PadLeft(8, '0'),2);//???
        await SendMessage(data);
    }

    private bool searchDone = false;
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

        data[44] = Convert.ToByte(real.PadLeft(8, '0'), 2);//???

        await SendMessage(data);
    }

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
        data[44] = Convert.ToByte(real.PadLeft(8, '0'), 2);//???
        await SendMessage(data);
    }

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
        data[44] = Convert.ToByte(real.PadLeft(8, '0'), 2);//???
        await SendMessage(data);
    }

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
        data[44] = Convert.ToByte(real.PadLeft(8, '0'), 2);//???
        await SendMessage(data);
    }

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
    // Modify 4/14/2017
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

internal class State1Object
{
    public State1Object() { }

    public State1Object(string imageEnhancement, string multiples)
    {
        this.imageEnhancement = imageEnhancement;
        this.multiples = multiples;
    }

    private string imageEnhancement;
    private string multiples;

    public string ImageEnhancement
    {
        get
        {
            return imageEnhancement;
        }

        set
        {
            imageEnhancement = value;
        }
    }

    public string Multiples
    {
        get
        {
            return multiples;
        }

        set
        {
            multiples = value;
        }
    }
}

internal class State0Object
{
    private string directionDrive;
    private string upDownDrive;
    private string light;
    public State0Object() { }
    public State0Object(string directionDrive, string upDownDrive, string light)
    {
        this.directionDrive = directionDrive;
        this.upDownDrive = upDownDrive;
        this.light = light;
    }

    public string DirectionDrive
    {
        get
        {
            return directionDrive;
        }

        set
        {
            directionDrive = value;
        }
    }

    public string UpDownDrive
    {
        get
        {
            return upDownDrive;
        }

        set
        {
            upDownDrive = value;
        }
    }

    public string Light
    {
        get
        {
            return light;
        }

        set
        {
            light = value;
        }
    }
}