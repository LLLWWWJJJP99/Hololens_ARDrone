using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClickHelper : MonoBehaviour , IInputClickHandler
{
    VideoPanelManager3 manager;

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
