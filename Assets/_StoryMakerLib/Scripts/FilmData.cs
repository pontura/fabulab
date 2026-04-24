using System;
using UnityEngine;


namespace Yaguar.StoryMaker.Editor
{
    [Serializable]
    public class FilmData
    {
        public int speed;
        public string id;
        public int localId;
        public int framecount;
        public Texture2D thumb;
        public string name;
        public string userID;
        public string username;
        public string timestamp;

        public Sprite GetSprite() {
            if (thumb == null) {
                Debug.LogError("No thumb for film id: " + id);
                return null;
            }
            return Sprite.Create(thumb, new Rect(0, 0, thumb.width, thumb.height), Vector2.zero);
        }
    }
}
