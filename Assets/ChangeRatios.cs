using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeRatios : MonoBehaviour , IInputClickHandler , IManipulationHandler
{
    private GoogleMap script;

    public void OnInputClicked(InputClickedEventData eventData)
    {
        script.zoominm();
    }

    public void OnManipulationCanceled(ManipulationEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnManipulationStarted(ManipulationEventData eventData)
    {
        script.zoomoutm();
    }

    public void OnManipulationUpdated(ManipulationEventData eventData)
    {
        throw new NotImplementedException();
    }



    // Use this for initialization
    void Start () {
        script = GameObject.Find("Canvas/PanelMap/Plane").GetComponent<GoogleMap>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
