using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour {
    private OffLineMap map1;
    private bool offLine = true;

    void Awake()
    {
        map1 = GameObject.Find("Canvas/PanelMap/Plane").GetComponent<OffLineMap>();
    }

    // Use this for initialization
    void Start () {

	}


	// Update is called once per frame
	void Update () {
		
	}
}
