using HoloLensXboxController;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using HoloToolkit.Unity;

public class VideoPanelManager3 : MonoBehaviour, IInputClickHandler
{
    // capture the click event
    private GameObject video;
    private GameObject mycamera;
    private GameObject map;
    private GameObject btns;
    private GameObject canvas;
    private Vector3 videoOriginalScale;
    private Vector3 videoOriginalPosition;
    private GameObject label;

    private GameObject myCameraInfo;
    private Vector3 cameraOriginalAngles;
    private Vector3 labelOriginalPosition;
    private Vector3 mapOriginalPosition;
    private Vector3 canvasOriginalPosition;
    private GameObject backGround;
    private Vector3 backGroundOriginalPosition;
    private InfoCanvasController info;
    private GazeManager manager;
    private Vector3[] positions = new Vector3[4];
    private Vector3 hitPosition;
    private GameObject point1;
    private GameObject point2;
    private GameObject point3;
    private GameObject point4;
    public Color c1 = Color.blue;
    public Color c2 = Color.black;
    private LineRenderer lineRenderer1;
    private LineRenderer lineRenderer2;
    private LineRenderer lineRenderer3;
    private LineRenderer lineRenderer4;
    private DemoSocket socket;
    private int selected = -1;
    private GameObject anchor;
    private float width = 80f;
    private Vector3 inversePos;
    private bool draw = false;
    private bool zoomup = false;
    private bool captured = false;
    private float ratio = 100f / 35f;
    private bool firstShowUp = false;
    //Use this for initialization

    void Start()
    {
        socket = GameObject.Find("Directional light").GetComponent<DemoSocket>();
        mycamera = GameObject.Find("HoloLensCamera");
        myCameraInfo = GameObject.Find("CameraInfo");
        label = myCameraInfo.transform.Find("rotation").gameObject;
        myCameraInfo.SetActive(true);
        label.SetActive(true);

        backGround = GameObject.Find("Canvas/BackgroundPanel");
        info = GameObject.Find("InfoCanvas").GetComponent<InfoCanvasController>();
        manager = GameObject.Find("InputManager").GetComponent<GazeManager>();
        video = GameObject.Find("Canvas/PanelVideo");
        map = GameObject.Find("Canvas/PanelMap");
        btns = GameObject.Find("Canvas/PanelBtns");
        anchor = video.transform.Find("anchor").gameObject;
        canvas = GameObject.Find("Canvas");
        point1 = GameObject.Find("point1");
        point2 = GameObject.Find("point2");
        point3 = GameObject.Find("point3");
        point4 = GameObject.Find("point4");

        lineRenderer1 = point1.AddComponent<LineRenderer>();
        lineRenderer1.material = new Material(Shader.Find("Particles/Additive"));

        lineRenderer1.startColor = c1;
        lineRenderer1.endColor = c2;
        lineRenderer1.startWidth = 0.01F;
        lineRenderer1.endWidth = 0.01F;
        lineRenderer1.positionCount = 2;

        lineRenderer2 = point2.AddComponent<LineRenderer>();
        lineRenderer2.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer2.startColor = c1;
        lineRenderer2.endColor = c2;
        lineRenderer2.startWidth = 0.01F;
        lineRenderer2.endWidth = 0.01F;
        lineRenderer2.positionCount = 2;

        lineRenderer3 = point3.AddComponent<LineRenderer>();
        lineRenderer3.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer3.startColor = c1;
        lineRenderer3.endColor = c2;
        lineRenderer3.startWidth = 0.01F;
        lineRenderer3.endWidth = 0.01F;
        lineRenderer3.positionCount = 2;

        lineRenderer4 = point4.AddComponent<LineRenderer>();
        lineRenderer4.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer4.startColor = c1;
        lineRenderer4.endColor = c2;
        lineRenderer4.startWidth = 0.01F;
        lineRenderer4.endWidth = 0.01F;
        lineRenderer4.positionCount = 2;

        lineRenderer1.enabled = false;
        lineRenderer2.enabled = false;
        lineRenderer3.enabled = false;
        lineRenderer4.enabled = false;
    }

    public void SendPeriodicalMessage()
    {
        float upDown = CalculateRotationXAngle();
        if (Mathf.Abs(upDown) <= 10)
        {
            upDown = 0;
        }

        float direction = CalculateRotationYAngle();
        if (Mathf.Abs(direction) <= 10)
        {
            direction = 0;
        }

        if (upDown > 10) {
            upDown = (upDown - 10f) * ratio;
        }
        else if (upDown<-10)
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

#if UNITY_UWP
        socket.SendSearchManually(direction,upDown);
#endif
    }

    public Vector3 InversePos
    {
        get
        {
            return inversePos;
        }

        set
        {
            inversePos = value;
        }
    }

    public float Width
    {
        get
        {
            return width;
        }

        set
        {
            width = value;
        }
    }

    public GameObject Anchor
    {
        get
        {
            return anchor;
        }

        set
        {
            anchor = value;
        }
    }

    public bool Captured
    {
        get
        {
            return captured;
        }

        set
        {
            captured = value;
        }
    }

    public Vector3 CameraOriginalAngles
    {
        get
        {
            return cameraOriginalAngles;
        }

        set
        {
            cameraOriginalAngles = value;
        }
    }

    public float CalculateRotationXAngle()
    {
        if (Vector3.Dot(new Vector3(0f, mycamera.transform.forward.y, mycamera.transform.forward.z), Quaternion.AngleAxis(90f, Vector3.right) * (new Vector3(0f, cameraOriginalAngles.y, cameraOriginalAngles.z))) > 0)
        {
            return -Vector3.Angle(new Vector3(0f, cameraOriginalAngles.y, cameraOriginalAngles.z), new Vector3(0f, mycamera.transform.forward.y, mycamera.transform.forward.z));
        }
        return Vector3.Angle(new Vector3(0f, cameraOriginalAngles.y, cameraOriginalAngles.z), new Vector3(0f, mycamera.transform.forward.y, mycamera.transform.forward.z));
    }

    public float CalculateRotationYAngle()
    {
        if (Vector3.Dot(new Vector3(mycamera.transform.forward.x, 0f, mycamera.transform.forward.z), Quaternion.AngleAxis(90f, Vector3.up) * (new Vector3(cameraOriginalAngles.x, 0f, cameraOriginalAngles.z))) < 0)
        {
            return -Vector3.Angle(new Vector3(cameraOriginalAngles.x, 0f, cameraOriginalAngles.z), new Vector3(mycamera.transform.forward.x, 0f, mycamera.transform.forward.z));
        }
        return Vector3.Angle(new Vector3(cameraOriginalAngles.x, 0f, cameraOriginalAngles.z), new Vector3(mycamera.transform.forward.x, 0f, mycamera.transform.forward.z));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (myCameraInfo!=null) {
            myCameraInfo.transform.rotation = mycamera.transform.rotation;
            myCameraInfo.transform.position = mycamera.transform.forward*5;
        }
        if (selected == 0)
        {
            if (manager.IsGazingAtObject)
            {
                hitPosition = manager.HitInfo.point;
                ShowRectangle(width, hitPosition);
            }
            if (socket.ControllerInput != null && (socket.ControllerInput.GetAxisRightThumbstickX() < 0 || socket.ControllerInput.GetAxisLeftThumbstickY() < 0))
            {
                width -= 10f;
            }
            else if (socket.ControllerInput != null && (socket.ControllerInput.GetAxisRightThumbstickX() > 0 || socket.ControllerInput.GetAxisLeftThumbstickY() > 0))
            {
                width += 10f;
            }
        }

        if (inversePos != null && draw && !zoomup && !captured)
        {
            Vector3 worldPos = anchor.transform.TransformPoint(inversePos);

            ShowRectangle(width, worldPos);
        }
        if (socket.Prepared)
        {
            Vector3[] vectors = socket.Vectors;
            Vector3 mid = (vectors[0] + vectors[1]) / 2;
            Vector3 worldPos = anchor.transform.TransformPoint(mid);
            ShowRectangle(width, worldPos);
            socket.Prepared = false;
        } else if (firstShowUp&& !socket.Prepared) {
            // draw the rectangle back to main menu
            Vector3 worldPos = anchor.transform.TransformPoint(inversePos);
            ShowRectangle(width, worldPos);
        }
    }

    public void DisableRectangle() {
        lineRenderer1.enabled = false;
        lineRenderer2.enabled = false;
        lineRenderer3.enabled = false;
        lineRenderer4.enabled = false;
    }

    public void ShowRectangle(float halfWidth,Vector3 pos) {
        inversePos = anchor.transform.InverseTransformPoint(pos);
        Vector3 v1 = new Vector3(inversePos.x - halfWidth, inversePos.y + halfWidth, inversePos.z);
        Vector3 v2 = new Vector3(inversePos.x + halfWidth, inversePos.y + halfWidth, inversePos.z);
        Vector3 v3 = new Vector3(inversePos.x - halfWidth, inversePos.y - halfWidth, inversePos.z);
        Vector3 v4 = new Vector3(inversePos.x + halfWidth, inversePos.y - halfWidth, inversePos.z);
        Vector3 w1 = anchor.transform.TransformPoint(v1);
        Vector3 w2 = anchor.transform.TransformPoint(v2);
        Vector3 w3 = anchor.transform.TransformPoint(v3);
        Vector3 w4 = anchor.transform.TransformPoint(v4);
        positions[0] = new Vector3(w1.x, w1.y, w1.z - 0.1f);
        positions[3] = new Vector3(w4.x, w4.y, w4.z - 0.1f);
        positions[2] = new Vector3(w3.x, w3.y, w3.z - 0.1f);
        positions[1] = new Vector3(w2.x, w2.y, w2.z - 0.1f);

        point1.transform.localPosition = positions[0];
        point2.transform.localPosition = positions[1];
        point3.transform.localPosition = positions[2];
        point4.transform.localPosition = positions[3];

        lineRenderer1.SetPosition(0, point1.transform.position);
        lineRenderer1.SetPosition(1, point2.transform.position);

        lineRenderer2.SetPosition(0, point2.transform.position);
        lineRenderer2.SetPosition(1, point4.transform.position);

        lineRenderer3.SetPosition(0, point3.transform.position);
        lineRenderer3.SetPosition(1, point1.transform.position);

        lineRenderer4.SetPosition(0, point4.transform.position);
        lineRenderer4.SetPosition(1, point3.transform.position);

        lineRenderer1.enabled = true;
        lineRenderer2.enabled = true;
        lineRenderer3.enabled = true;
        lineRenderer4.enabled = true;

    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (selected == 0) {
            FindCoordinate();
            selected = 1;
        } else if (selected==1) {
            canvas.transform.localPosition = canvasOriginalPosition;
            map.transform.localPosition = mapOriginalPosition;
            video.transform.localPosition = videoOriginalPosition;
            backGround.transform.localPosition = backGroundOriginalPosition;
            video.transform.localScale = videoOriginalScale;
            draw = true;
            selected = -1;
            captured = true;


            // enbale tagalong following
            canvas.GetComponent<SimpleTagalong>().enabled = true;
            canvas.GetComponent<Billboard>().enabled = true;
        }
        else if (selected==-1) {
            // disable tagalong following
            canvas.GetComponent<SimpleTagalong>().enabled = false;
            canvas.GetComponent<Billboard>().enabled = false;
            captured = false;
            DisableRectangle();
            canvasOriginalPosition = canvas.transform.localPosition;
            mapOriginalPosition = map.transform.localPosition;
            videoOriginalPosition = video.transform.localPosition;
            backGroundOriginalPosition = backGround.transform.localPosition;
            videoOriginalScale = video.transform.localScale;
            video.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            float tmpY = canvas.transform.position.y;
            canvas.transform.localPosition = new Vector3(mycamera.transform.forward.x * 994.0f, tmpY, mycamera.transform.forward.z * 994.0f);//????
            video.transform.position = new Vector3(mycamera.transform.forward.x * 5.0f, tmpY, mycamera.transform.forward.z * 5.0f);
            selected = 0;
            width = 80f;
            draw = false;
            firstShowUp = true;
        }

    }

    public void FindCoordinate()
    {   //If videopanel has not been clicked before but is clicked now, zoom it up
        if (manager.IsGazingAtObject)
        {
            hitPosition = manager.HitInfo.point;
            Vector3 relativePosition = anchor.transform.InverseTransformPoint(hitPosition);
            float x = Mathf.Abs(relativePosition.x);
            float y = Mathf.Abs(relativePosition.y);
            Debug.Log(" X " + x + " axis| Y " + y + " axis ");
#if UNITY_UWP
            for (int i = 0;i < 3;i++) {
                socket.SendCoordinateInfo(x-width, y+width, x+width, y-width);
            }
#endif
        }
    }
    public void BindCanvasToCamera()
    {   //If videopanel has not been clicked before but is clicked now, zoom it up
        DisableRectangle();
        zoomup = true;
        canvasOriginalPosition = canvas.transform.localPosition;
        mapOriginalPosition = map.transform.localPosition;
        videoOriginalPosition = video.transform.localPosition;
        backGroundOriginalPosition = backGround.transform.localPosition;
        videoOriginalScale = video.transform.localScale;
        video.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        btns.SetActive(false);
        map.transform.localPosition = new Vector3(0f, -0.22f, 994.0f);
        backGround.transform.localPosition = new Vector3(0f, -0.22f, 994.0f);

        canvas.transform.SetParent(mycamera.transform, false);
        canvas.transform.localPosition = new Vector3(0f, -0.22f, 9.0f);
        video.transform.SetParent(mycamera.transform.Find("CanvasForVideo").transform, false);
        video.transform.localPosition = new Vector3(0f, 0, 3.0f);
        DisplayLabel();
    }
    public void ReleaseCanvas() {
        map.transform.localPosition = mapOriginalPosition;
        backGround.transform.localPosition = backGroundOriginalPosition;
        btns.SetActive(true);
        video.transform.localScale = videoOriginalScale;
        video.transform.localPosition = videoOriginalPosition;
        video.transform.SetParent(canvas.transform, false);
        Vector3 tmpPosition = canvas.transform.position;
        Vector3 tmpScale = canvas.transform.lossyScale;
        canvas.transform.parent = null;
        canvas.transform.position = tmpPosition;
        canvas.transform.localScale = tmpScale;
        canvas.transform.localRotation = mycamera.transform.localRotation;
        HideLabel();
        zoomup = false;
    }

    public void DisplayLabel()
    {
        if (label != null)
        {
            labelOriginalPosition = label.transform.localPosition;
            label.transform.localPosition = new Vector3(0f, -120f, 2.5f);
            label.transform.SetParent(mycamera.transform.Find("CanvasForVideo").transform, false);
        }

    }

    public void HideLabel()
    {
        if (label != null)
        {
            label.transform.localPosition = labelOriginalPosition;
            label.transform.SetParent(myCameraInfo.transform, false);
        }
    }
}

