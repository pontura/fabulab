using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

public class TimelineInSceneUIFabulab : TimelineInSceneUI
{
    public override void RefreshField(int sceneID)
    {
        field.text = sceneID.ToString();

      //  field.text = sceneID + "/" + ScenesManagerFabulab.Instance.Scenes.Count + " ("+ (ScenesManagerFabulab.Instance.MaxKeyframes - ScenesManagerFabulab.Instance.Scenes.Count) +")";
    }
}
