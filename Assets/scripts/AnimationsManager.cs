using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimationsManager : MonoBehaviour
{
    public enum anim
    {
        NONE,
		ROTATE_FAST,
		LATIDO,
		ROTATE_HORIZONTAL_FAST,
		HAMACA,
		ROTATE_SOFT,
        BOING,
        VA_Y_VIENE,
		ENGINE_1,
		ENGINE_2,
		ENGINE_3,
		ENGINE_4,
		INSIDEOUT,
		NOPE,
		NOPE_SLOW,
		ROTATION_2,
		ROTATION_4,
		SHAKE,
		SHRINK,
		ELECTRIC,
		BALL_1,
		BALL_2,
		LOL,
		MECHA_1,
		MECHA_2,
		MECHA_3,
		MECHA_4,
		WOW,
		SHIP

    }
	

    [Serializable]
    public class AnimData
    {
        public anim animName;
        public AnimationClip clip;
        public Sprite icon;
    }
    public AnimData[] all;
    public AnimData GetAnimByName(anim animName)
    {
        foreach (AnimData animData in all)
            if (animData.animName == animName)
                return animData;
        return null;
    }
}
