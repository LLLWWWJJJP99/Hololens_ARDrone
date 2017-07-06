using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TouchMe : MonoBehaviour
{
    private VideoPanelManager3 manager;

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
