using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class click : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject btnObj = GameObject.Find("Button");
        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(delegate ()
        {
            SceneManager.LoadScene("Holo_testmap");
        });
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
