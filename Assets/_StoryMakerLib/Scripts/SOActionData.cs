using System.Collections;
using System;
using UnityEngine;

[Serializable]
public class SOActionData : SOData
{
    public int expressionID;
    public Sprite icon;
    public types type;
    public enum types
    {
        IDLE,
        CHOMP,
        CRY,
        DEAD,
        KISS_LOVE,
        LOL,
        LYNA_BEAR,
        RAGE,
        SHAME_HAPPY,
		SIT_HAPPY,
        SMILE_NERVOUS,
        SOB,
        SUSPICIOUS,
        SWEAT_SMILE,
        WOW,
        YUM,
        BOSS,
        JUMP,
        SIT,
        HELLO,
		WALK,
		RUN,
		DANCE1,
		WEAVE,
		WOW_HEAD
    }
    public AnimationClip clip;
}
