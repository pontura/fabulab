 ﻿using UnityEngine;
 using System.Collections;
 using System.Collections.Generic;
 
 public static class Utils {
 
     public static void RemoveAllChildsIn(Transform container)
     {
         int num = container.transform.childCount;
         for (int i = 0; i < num; i++) UnityEngine.Object.DestroyImmediate(container.transform.GetChild(0).gameObject);
     }
     public static void ShuffleListTexts(List<string> texts)
     {
         if (texts.Count < 2) return;
         for (int a = 0; a < 100; a++)
         {
             int id = Random.Range(1, texts.Count);
             string value1 = texts[0];
             string value2 = texts[id];
             texts[0] = value2;
             texts[id] = value1;
         }
     }
    public static float GetAngleBetween(Vector3 startPoint, Vector3 endPoint)
    {
        float angle = Mathf.Atan2((endPoint.y - startPoint.y), (endPoint.x - startPoint.x));
        angle *= Mathf.Rad2Deg;
        return angle;
    }
}
