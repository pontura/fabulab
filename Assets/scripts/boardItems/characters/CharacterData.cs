using System;
using static BoardItems.Characters.CharacterData;

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

        public static int GetServerPartsLength() {
            return Enum.GetValues(typeof(parts)).Length - 2;
        }       

        public static string GetServerPartsId(int partId) {
            if (partId == 6)
                partId = 3;
            if (partId == 5)
                partId = 4;
            return ((parts)partId).ToString();
        }
    }
}
