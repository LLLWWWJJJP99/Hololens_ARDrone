using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class GazeHelper : MonoBehaviour , IFocusable
{
    public Sprite newImage;
    private Sprite oldImage;
    public void OnFocusEnter()
    {
        this.gameObject.GetComponent<Button>().image.sprite = newImage;
    }

    public void OnFocusExit()
    {
        this.gameObject.GetComponent<Button>().image.sprite = oldImage;
    }

    // Use this for initialization
    void Start () {
        oldImage = this.gameObject.GetComponent<Button>().image.sprite;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
