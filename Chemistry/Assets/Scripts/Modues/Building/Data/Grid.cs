using UnityEngine;
using System.Collections;

public class Grid {
    public Vector3 pos;
    public bool enable;
    public float scale = 0.9f;
    public Grid(bool enable)
    {
        this.enable = enable;
    }
}
public class Rectangle
{
    public Vector3 pos;
    public bool enable;
    public float widght;
    public float length;

    public Rectangle(float widght,float height, bool enable)
    {
        this.widght = widght;
        this.length = height;
        this.enable = enable;
    }
}
