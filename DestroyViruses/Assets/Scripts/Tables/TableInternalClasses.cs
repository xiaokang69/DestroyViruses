using System;
using UnityEngine;

[Serializable]
public struct TColor
{
    private float r;
    private float g;
    private float b;
    private float a;

    public static TColor Parse(string str)
    {
        ColorUtility.TryParseHtmlString(str, out Color color);
        var data = new TColor
        {
            r = color.r,
            g = color.g,
            b = color.b,
            a = color.a
        };
        return data;
    }

    public Color value => new Color(r, g, b, a);
}

[Serializable]
public struct TQuaternion
{
    private float x;
    private float y;
    private float z;

    public static TQuaternion Parse(string str)
    {
        var sps = str.Trim('(', ')').Split(',');
        var data = new TQuaternion
        {
            x = float.Parse(sps[0]),
            y = float.Parse(sps[1]),
            z = float.Parse(sps[2])
        };
        return data;
    }

    public Quaternion value => Quaternion.Euler(x, y, z);
}

[Serializable]
public struct TVector3
{
    private float x;
    private float y;
    private float z;

    public static TVector3 Parse(string str)
    {
        var sps = str.Trim('(', ')').Split(',');
        var data = new TVector3
        {
            x = float.Parse(sps[0]),
            y = float.Parse(sps[1]),
            z = float.Parse(sps[2])
        };
        return data;
    }

    public Vector3 value => new Vector3(x, y, z);
}

[Serializable]
public struct TVector2
{
    private float x;
    private float y;

    public static TVector2 Parse(string str)
    {
        var sps = str.Trim('(', ')').Split(',');
        var data = new TVector2
        {
            x = float.Parse(sps[0]),
            y = float.Parse(sps[1]),
        };
        return data;
    }

    public Vector2 value => new Vector2(x, y);
}

[Serializable]
public struct TRange
{
    public float min { get; private set; }
    public float max { get; private set; }
    public float random { get { return UnityEngine.Random.Range(min, max); } }
    
    public bool Contains(float value)
    {
        return value >= min && value <= max;
    }

    public static TRange Parse(string str)
    {
        var sps = str.Trim('(', ')').Split('~', ',');
        var data = new TRange();
        data.min = float.Parse(sps[0]);
        if (sps.Length <= 1) data.max = data.min;
        else data.max = float.Parse(sps[1]);
        return data;
    }
}

[Serializable]
public struct TRangeInt
{
    public int min { get; private set; }
    public int max { get; private set; }
    public int random { get { return UnityEngine.Random.Range(min, max + 1); } }
    
    public bool Contains(int value)
    {
        return value >= min && value <= max;
    }
    
    public static TRangeInt Parse(string str)
    {
        var sps = str.Trim('(', ')').Split('~', ',');
        var data = new TRangeInt();
        data.min = int.Parse(sps[0]);
        if (sps.Length <= 1) data.max = data.min;
        else data.max = int.Parse(sps[1]);
        return data;
    }
}
