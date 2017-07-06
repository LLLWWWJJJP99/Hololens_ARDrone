using UnityEngine;
using System.Collections;
using HoloToolkit.Unity;

public class GoogleMap : MonoBehaviour
{
	public enum MapType
	{
		RoadMap,
		Satellite,
		Terrain,
		Hybrid
	}
    private bool done = true;
    private bool loadOnStart = false;
	public bool autoLocateCenter = false;
	public GoogleMapLocation centerLocation;
	int zoom = 8;
	public MapType mapType;
	public int size = 512;
	public bool doubleResolution = false;
    float lonti = 116.4f, al = 39.9f;
    string path = "";
    string[] dots = new string[6];
    int count = 0;
    void Start() {
            StartRefresh();
            //Refresh();
    }
    public void StartRefresh() {
        StartCoroutine(_Refresh());
    }

    public void ChangeStatusToTrue() {
        loadOnStart = true;
    }
    public void ChangeStatusToFalse()
    {
        loadOnStart = false;
    }
    void Update()
    {

    }

    public void zoominm()
    {
        zoom += 2;
        if (done)
        { StartCoroutine(_Refresh()); }// modified 0705 2017
    }

    public void zoomoutm()
    {
        zoom -= 2;
        StartCoroutine(_Refresh()); // modified 0705 2017
    }

    IEnumerator _Refresh()
    {
        while (done)
        {
            yield return new WaitForSecondsRealtime(2f);
            var url = "https://maps.googleapis.com/maps/api/staticmap?key=AIzaSyCidb7dqdHMEv-t2ZOIduPX5_V_UUiQS-I&size=640x640";
            var qs = "";

            qs += "&center=" + Al.ToString() + "," + Lonti.ToString();
            qs += "&zoom=" + zoom.ToString();
            if (count == dots.Length) count = 0;
            dots[count++] = "%7C" + Al.ToString() + "," + Lonti.ToString();

            qs += "&markers=color:blue%7Clabel:S";
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] != null && !dots[i].Equals(""))
                {
                    qs += dots[i];
                }
            }
            qs += "&path=color:0x0000ff%7Cweight:5";
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] != null && !dots[i].Equals(""))
                {
                    qs += dots[i];
                }
            }
            var req = new WWW(url + qs);
            yield return req;
            GetComponent<Renderer>().material.mainTexture = req.texture;
        }

    }



    public float Lonti
    {
        get
        {
            return lonti;
        }

        set
        {
            lonti = value;
        }
    }

    public float Al
    {
        get
        {
            return al;
        }

        set
        {
            al = value;
        }
    }

    public bool LoadOnStart
    {
        get
        {
            return loadOnStart;
        }

        set
        {
            loadOnStart = value;
        }
    }

    public bool Done
    {
        get
        {
            return done;
        }

        set
        {
            done = value;
        }
    }

}


public enum GoogleMapColor
{
	black,
	brown,
	green,
	purple,
	yellow,
	blue,
	gray,
	orange,
	red,
	white
}

[System.Serializable]
public class GoogleMapLocation
{
	public string address;
	public float latitude;
	public float longitude;
}

[System.Serializable]
public class GoogleMapMarker
{
	public enum GoogleMapMarkerSize
	{
		Tiny,
		Small,
		Mid
	}
	public GoogleMapMarkerSize size;
	public GoogleMapColor color;
	public string label;
	public GoogleMapLocation[] locations;
	
}

[System.Serializable]
public class GoogleMapPath
{
	public int weight = 5;
	public GoogleMapColor color;
	public bool fill = false;
	public GoogleMapColor fillColor;
	public GoogleMapLocation[] locations;	
}