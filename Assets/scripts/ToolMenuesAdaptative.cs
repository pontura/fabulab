using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolMenuesAdaptative : MonoBehaviour
{
    public GameObject all;
    public float smooth;

    public void Init()
    {
        float _y =  transform.GetComponent<RectTransform>().anchoredPosition.y;
        Vector2 pos = all.transform.localPosition;
        pos.y = _y/smooth;
        all.transform.localPosition = pos;
    }
}
