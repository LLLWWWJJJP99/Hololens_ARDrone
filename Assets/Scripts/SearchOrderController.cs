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
        if (text != null && mycamera != null && startSearchOrder)
        {
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
            text.text = " 上下旋转角:" + Mathf.Round(upDown )
            + "度| 左右旋转角:" + Mathf.Round(direction ) + "度";
        } else if (text != null&&!startSearchOrder) {
            text.text = "";
        }

        if (startSearchOrder){
            videoManager.SendPeriodicalMessage();
        }

    }

    public void StartSearch() {
        videoManager.CameraOriginalAngles = mycamera.transform.forward;
        startSearchOrder = true;
    }

    public void StopSearch() {
        startSearchOrder = false;
    }

    public void HoverOrder() {
#if UNITY_UWP
        for (int i = 0;i<3;i++) {
            StartCoroutine(WaitForSixtyMS());
            socket.SendHoverOrder();
        }
#endif
    }
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
