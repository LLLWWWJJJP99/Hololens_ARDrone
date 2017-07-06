using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MainBtnController : MonoBehaviour
{
    private GameObject canvas;
    private GameObject infocanvas;
    private GameObject[] displays;
    public void showSinglePanel(GameObject panel)
    {
        panel.transform.SetAsLastSibling();
    }

    // Use this for initialization
    void Start()
    {
        infocanvas = GameObject.Find("InfoCanvas");
        canvas = GameObject.Find("Canvas");
    }

    // Update is called once per frame
    void Update()
    {

    }


}
