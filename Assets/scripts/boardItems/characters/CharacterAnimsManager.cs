using System;
using UnityEngine;

public class CharacterAnimsManager : MonoBehaviour
{
    public AnimationClip defaultIdle;
    public AnimationClip defaultEdit;
    public AnimationClip defaultWalk;
    public AnimationClip defaultRun;
    public AnimationClip defaultEmoji;

    public AnimData[] all;
    [Serializable]
    public class AnimData
    {
        public string name;
        public AnimationClip clip;
    }

    public EmojiData[] emojis;
    [Serializable]
    public class EmojiData
    {
        public string name;
        public AnimationClip clip;
    }
}
