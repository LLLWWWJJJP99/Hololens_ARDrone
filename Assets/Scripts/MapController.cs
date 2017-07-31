using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour {
    //private GameObject mapPanel;

    private List<Vector2> coordinates = new List<Vector2>();
    private bool offLine = true;
    private static MapController controller;
    private GameObject anchor;
    private List<GameObject> dots = new List<GameObject>();
    private int count = 0;
    private GameObject cube;
    private GameObject plane;
    private int num = 0;
    public Color c1;
    public Color c2;
    private Vector2 resolution1 = new Vector2(512,256);
    private Vector2 resolution2 = new Vector2(1536, 1024);
    private Vector2 resolution3 = new Vector2(5120, 3328);
    private Vector2 resolution4 = new Vector2(19456, 12544);
    private Vector2 current = new Vector2(512, 256);
    public List<GameObject> Dots
    {
        get
        {
            return dots;
        }

        set
        {
            dots = value;
        }
    }

    public int Count
    {
        get
        {
            return count;
        }

        set
        {
            count = value;
        }
    }

    public List<Vector2> Coordinates
    {
        get
        {
            return coordinates;
        }

        set
        {
            coordinates = value;
        }
    }

    void Awake()
    {
        //下面的数据分别是每张图我估计的左下角，右上角的点经纬度。
        //119.364251f, 31.4978475f 左下 l12
        //119.518746f, 31.575095f 右上 l12

        //31.573395, 119.472891 右上 l15
        //31.537557, 119.406458 左下 l15

        //31.576832, 119.455940 右上 l17
        //31.548017, 119.402167 左下 l17

        //31.576539, 119.453193 右上 l19
        //31.548017, 119.400836 左下 l19

        controller = this;
        plane = GameObject.Find("Canvas/PanelMap/Plane");
        //cube是地图上将要放置的点，刚开始用错名字我就懒得换了
        cube = GameObject.Find("Cube");
        anchor = GameObject.Find("Canvas/PanelMap/Anchor");


    }

    // Use this for initialization
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < dots.Count; i++)
        {
            //在地图上画线
            if (dots[i] != null)
            {
                if (i < dots.Count - 1 && dots[i + 1] != null)
                {
                    dots[i].GetComponent<LineRenderer>().SetPosition(0, dots[i].transform.position);
                    dots[i].GetComponent<LineRenderer>().SetPosition(1, dots[i + 1].transform.position);
                    dots[i].GetComponent<LineRenderer>().enabled = true;
                }
            }
        }
    }

    public static MapController GetMapController()
    {
        return controller;
    }

    public void StartLoadMap() {
#if WINDOWS_UWP
        SaveLoadFile.GetSaveLoadFile().ReadData();
#endif
    }
    //在接收到upd package后 会把解析得到的经纬度传给这个函数，这个函数在地图上设置点的位置，还有线的参数。
    public void AddPoints(float longti, float lati)
    {
        if (count == 6)
        {
            count = 5;
            coordinates.RemoveAt(0);
            GameObject first = dots[0];
            dots.RemoveAt(0);
            Destroy(first);
        }
        dots.Add(Instantiate(cube));
        coordinates.Add(new Vector2(longti, lati));
        MovePointsToRealPosition(longti,lati,count);
        dots[count].transform.SetParent(plane.transform, true);
        LineRenderer line = dots[count].AddComponent<LineRenderer>();
        line.enabled = false;
        line.material = new Material(Shader.Find("Particles/Additive"));

        line.startColor = c1;
        line.endColor = c2;
        line.startWidth = 0.01F;
        line.endWidth = 0.01F;
        line.positionCount = 2;

        count++;
    }
    //这个函数用来根据经纬度得到点在mappanel中的坐标，再根据坐标设置点的世界位置。之所以和上面的函数分开，是因为切换分辨率时，需要根据保存的各个点的经纬度，重新计算
    //点的世界位置，所以在点击放大缩小分辨率时，会重新触发这个函数一次。
    public void MovePointsToRealPosition(float longti,float lati,int index) {
        //CoordinateInfo coordinate = Caculate(lbj, lbw, longti, lati, rtj, rtw, 1080f, 1080f);
        //Vector3 inversePos = new Vector3(coordinate.X, coordinate.Y, 0f);
        //Debug.Log("inversePos "+inversePos);
        //Vector3 worldPos = anchor.transform.TransformPoint(inversePos);
        //dots[index].transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.z - 0.05f);
  
        //The data below is the test examples for latitute and lontitude for the map with the lowest resolution
        //119.4414985 31.5364712 |47315389 12CC1568 ||
        //119.3642510 31.5750949 |47258A0E 12D1FA25 ||
        //119.5187459 31.4978474 |473D1D03 12C630AA ||
        //119.5087459 31.5264712 |473B9663 12CA8EC8 ||
        //119.4725101 31.5078474 |47360EED 12C7B74A ||
        //119.5157459 31.5000000 |473CA7D3 12C684C0 ||
        //119.5100000 31.4900000 |473BC760 12C4FE20 ||
        //data below is  reversal of the hex data above
        //89533147 6815CC12
        //0E8A2547 25FAD112
        //031D3D47 AA30C612
        //63963B47 C88ECA12
        //ED0E3647 4AB7C712
        //D3A73C47 C084C612
        //60C73B47 20FEC412
    }

    //w0,j0是 方框中左下角的经纬度。j1 w1是方框中解析得到的点的经纬度。w2 ,j2是右上角经纬度。cx ,cy是主菜单中map panel的长宽分辨率。这个函数用来求解析
    //得到的点在map panel中的坐标。
    public CoordinateInfo Caculate(float j0, float w0, float j1, float w1, float j2, float w2, float cx, float cy)
    {
        float y = cy * (w1 - w0) / (w2 - w0);
        float x = cx * (j1 - j0) / (j2 - j0);
        CoordinateInfo info = new CoordinateInfo(x, y);
        return info;
    }

}
