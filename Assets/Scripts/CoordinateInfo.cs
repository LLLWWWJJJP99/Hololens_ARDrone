using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateInfo {

    private float x;
    private float y;

    public CoordinateInfo(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public float X
    {
        get
        {
            return x;
        }

        set
        {
            x = value;
        }
    }

    public float Y
    {
        get
        {
            return y;
        }

        set
        {
            y = value;
        }
    }
}
