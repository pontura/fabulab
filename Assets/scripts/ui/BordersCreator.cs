using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;

[RequireComponent(typeof(SpriteRenderer))]
public class BordersCreator : MonoBehaviour
{
    public float lineWidth = 3f;

    LineRenderer lr;
    SpriteRenderer sr;


    public Color color = Color.white;

    LineRenderer top, bottom, left, right;
    public void Show(bool isOn)
    {
        top.enabled = isOn;
        bottom.enabled = isOn;
        left.enabled = isOn;
        right.enabled = isOn;

      
    }
    public void Init (SpriteRenderer sr)
    {
        this.sr = sr;
        Bounds b = sr.bounds;

        top = CreateLine("Top");
        bottom = CreateLine("Bottom");
        left = CreateLine("Left");
        right = CreateLine("Right");

        Vector3 min = b.min;
        Vector3 max = b.max;

        // Top
        top.SetPosition(0, new Vector3(min.x, max.y, 0));
        top.SetPosition(1, new Vector3(max.x, max.y, 0));

        // Bottom
        bottom.SetPosition(0, new Vector3(min.x, min.y, 0));
        bottom.SetPosition(1, new Vector3(max.x, min.y, 0));

        // Left
        left.SetPosition(0, new Vector3(min.x, min.y, 0));
        left.SetPosition(1, new Vector3(min.x, max.y, 0));

        // Right
        right.SetPosition(0, new Vector3(max.x, min.y, 0));
        right.SetPosition(1, new Vector3(max.x, max.y, 0));
    }

    LineRenderer CreateLine(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(sr.transform);

        Camera cam = Camera.main;

        float worldHeight = cam.orthographicSize * 2f;
        float worldPerPixel = worldHeight / Screen.height;

        float worldWidth = lineWidth * worldPerPixel;

        // compensar el scale del objeto
        float compensatedWidth = worldWidth / go.transform.lossyScale.x;

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = compensatedWidth;
        lr.endWidth = compensatedWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.useWorldSpace = false;
        lr.sortingOrder = 100;
        return lr;
    }
}