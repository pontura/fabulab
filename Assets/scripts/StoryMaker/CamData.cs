using System;
using UnityEditor.XR;
using UnityEngine;

[Serializable]
public class CamData
{
    public Vector2 pos;
    public float zoom;
    public string name;
    public bool tween;

    public bool HasData() {return zoom != 0; }
}
