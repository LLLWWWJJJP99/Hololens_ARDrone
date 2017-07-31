using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClickHelper : MonoBehaviour , IInputClickHandler
{
    VideoPanelManager3 manager;
    //下面函数在视频区域被固定放大到摄像机前时，若再点击一次视频区域就会将视频区域放回主菜单，并将主菜单呈现到摄像机正面。
    public void OnInputClicked(InputClickedEventData eventData)
    {
        manager.ReleaseCanvas();
    }

    // Use this for initialization
    void Start () {
        manager = GameObject.Find("Canvas/PanelVideo").GetComponent<VideoPanelManager3>()  ;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
