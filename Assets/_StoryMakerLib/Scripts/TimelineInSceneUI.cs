using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

public class TimelineInSceneUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI field;
    public void Init()
    {
        RefreshField(0);
    }

    public void RefreshField(int sceneID)
    {
        field.text = sceneID + "/" + ScenesManager.Instance.GetTotalScenes() + " ("+ (ScenesManager.Instance.MaxKeyframes - ScenesManager.Instance.GetTotalScenes()) +")";
    }
}
