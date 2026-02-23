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

        public Sprite GetSprite() {
            if (thumb == null) {
                Debug.LogError("No hay thumb para character id: " + id);
                return null;
            }
            return Sprite.Create(thumb, new Rect(0, 0, thumb.width, thumb.height), Vector2.zero);
        }
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