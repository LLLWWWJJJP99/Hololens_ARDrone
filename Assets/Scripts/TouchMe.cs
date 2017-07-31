using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TouchMe : MonoBehaviour
{
    private VideoPanelManager3 manager;
    //通过语音指令back，放大video panel，并且固定在摄像机前。之后配合ClickHelper把video panel放回主菜单
    public void CanvasBackToVision() {
        manager.BindCanvasToCamera();
    }
    // Use this for initialization
    void Start () {
        manager = GameObject.Find("Canvas/PanelVideo").GetComponent<VideoPanelManager3>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
