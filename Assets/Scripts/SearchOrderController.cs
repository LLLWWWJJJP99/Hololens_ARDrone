using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchOrderController : MonoBehaviour {
    private Text text;
    private bool startSearchOrder = false;
    private GameObject videopanel;
    private VideoPanelManager3 videoManager;
    private GameObject mycamera;
    private DemoSocket socket;
    private float ratio = 100f / 35f;
    public bool StartSearchOrder
    {
        get
        {
            return startSearchOrder;
        }

        set
        {
            startSearchOrder = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        videopanel = GameObject.Find("Canvas/PanelVideo");
        videoManager = videopanel.GetComponent<VideoPanelManager3>();
        mycamera = GameObject.Find("HoloLensCamera");
        text = GameObject.Find("CameraInfo/rotation/rotationtext").gameObject.GetComponent<Text>();
        socket = GameObject.Find("Directional light").GetComponent<DemoSocket>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //计算水平，左右方向的旋转角
        float upDown = videoManager.CalculateRotationXAngle();
        if (Mathf.Abs(upDown) <= 10)
        {
            upDown = 0;
        }

        float direction = videoManager.CalculateRotationYAngle();
        if (Mathf.Abs(direction) <= 10)
        {
            direction = 0;
        }

        if (upDown > 10)
        {
            upDown = (upDown - 10f) * ratio;
        }
        else if (upDown < -10)
        {
            upDown = (upDown + 10f) * ratio;
        }

        if (direction > 10)
        {
            direction = (direction - 10f) * ratio;
        }
        else if (direction < -10)
        {
            direction = (direction + 10f) * ratio;
        }

        upDown = Mathf.Round(upDown);
        direction = Mathf.Round(direction);
        //显示旋转角
        if (text != null&& startSearchOrder)
        {
            text.text = " 上下旋转角:" + upDown
            + "度| 左右旋转角:" + direction + "度";
        } else if (text != null&&!startSearchOrder) {
            text.text = "";
        }

        //开始发送周期性指令,也就是目标搜索指令
        if (startSearchOrder){
            videoManager.SendPeriodicalMessage(upDown, direction);
        }

    }
    //语音指令start触发这个函数，开启目标搜索
    public void StartSearch() {
        videoManager.CameraOriginalAngles = mycamera.transform.forward;
        startSearchOrder = true;
    }
    //停止发送目标搜索指令
    public void StopSearch() {
        startSearchOrder = false;
    }
    //发送悬停指令
    public void HoverOrder() {
#if UNITY_UWP
        for (int i = 0;i<3;i++) {
            StartCoroutine(WaitForSixtyMS());
            socket.SendHoverOrder();
        }
#endif
    }
    //发送半自动指令
    public void HalfAutoOrder()
    {
#if UNITY_UWP
        for (int i = 0; i < 3; i++)
        {
            StartCoroutine(WaitForSixtyMS());
            socket.SendHalfAutoOrder();
        }
#endif
    }
    //发送全自动指令
    public void CompleteAutoOrder()
    {
#if UNITY_UWP
        for (int i = 0; i < 3; i++)
        {
            StartCoroutine(WaitForSixtyMS());
            socket.SendCompleteAutoOrder();
        }
#endif
    }

    IEnumerator WaitForSixtyMS()
    {
        yield return new WaitForSecondsRealtime(0.02f);
        
    }
}
