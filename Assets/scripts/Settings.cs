using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public float maxScale;
    public float minScale;
    public int snapAngle;
    public float snapGride = 0.512f;
    public float snapScale = 0.025f;
    
    public CamData[] camDatas;
    public int[] limitZooms = {0,70};
    
    public int GetLimit(int id)
    {
        return limitZooms[id];
    }
}
