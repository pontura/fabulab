using System;

namespace BoardItems.Characters
{
    [Serializable]
    public static class CharacterData
    {
        public enum parts
        {
            none,
            HEAD = 1,
            BODY = 2,
            HAND = 3,
            FOOT = 4,
            FOOT_LEFT = 5,
            HAND_LEFT = 6,
            HAIR = 7,
            FACE = 8
        }
    }
}
