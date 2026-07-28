using System;
using UnityEditor.XR;
using UnityEngine;

[Serializable]
public class CamData
{
    public Vector2 pos;
    public int zoom;
    public string name;

    public bool HasData() {return zoom != 0; }
}
