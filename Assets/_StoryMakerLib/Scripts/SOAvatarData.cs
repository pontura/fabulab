using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SOAvatarData : SOData
{
    public SOActionData.types actionType;
    public int expressionID;
    public Sprite icon;
    public RenderTexture renderTexture;

    public void SetIcon()
    {
        icon = Resources.Load<Sprite>(itemName) as Sprite;
    }

}
