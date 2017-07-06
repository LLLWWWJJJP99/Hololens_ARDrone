using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class InfoCanvasController : MonoBehaviour {
    private GameObject mycamera;
    private GameObject canvas;
    private GameObject infocanvas;
    // Use this for initialization
    void Start () {
        mycamera = GameObject.Find("HoloLensCamera");
        infocanvas = GameObject.Find("InfoCanvas");
        canvas = GameObject.Find("Canvas");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void backMainCanvas()
    {
        infocanvas.GetComponent<RectTransform>().localPosition = new Vector3(mycamera.transform.forward.x * 1001,-0.65f, mycamera.transform.forward.z * 1001);
        canvas.GetComponent<RectTransform>().localPosition = new Vector3(mycamera.transform.forward.x * 9.0f, 0.0f, mycamera.transform.forward.z * 9.0f);
        canvas.transform.rotation = new Quaternion(0.0f, mycamera.transform.rotation.y,0.0f, mycamera.transform.rotation.w);
    }
    public void backInfoCanvas() {
        infocanvas.GetComponent<RectTransform>().localPosition = new Vector3(mycamera.transform.forward.x * 1001, -0.65f, mycamera.transform.forward.z * 1001);
    }
}
