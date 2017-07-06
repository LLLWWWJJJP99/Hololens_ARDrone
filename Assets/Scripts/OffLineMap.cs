using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffLineMap : MonoBehaviour {

    private Texture [] textures = new Texture[4];
    private int index = 0;
    private Renderer mainRender;
	// Use this for initialization
	void Start () {
        mainRender = this.gameObject.GetComponent<Renderer>();
        textures[0] = Resources.Load("maps/L12") as Texture;
        textures[1] = Resources.Load("maps/L15") as Texture;
        textures[2] = Resources.Load("maps/L17") as Texture;
        textures[3] = Resources.Load("maps/L19") as Texture;
        LoadFirstMap();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadFirstMap() {
        Debug.Log("LoadFirstMap"+textures[index]+"|");
        mainRender.material.mainTexture = textures[index];
    }

    public void IncreaseResolution()
    {
        if (index == 3) index = 2;
        mainRender.material.mainTexture = textures[++index];
    }

    public void DecreaseResolution()
    {
        if (index == 0) index = 1;
        mainRender.material.mainTexture = textures[--index];
    }
}
