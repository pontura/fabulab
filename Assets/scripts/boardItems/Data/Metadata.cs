using System;
using UnityEngine;

namespace BoardItems.BoardData
{
    [Serializable]
    public class CharacterMetaData
    {
        public string id;
        public Texture2D thumb;
        public string userID;
    }

    [Serializable]
    public class ServerCharacterMetaData
    {
        public string thumb;
        public string userID;
    }

    [Serializable]
    public class ServerPartMetaData
    {
        public string thumb;
        public string partID;
    }

}