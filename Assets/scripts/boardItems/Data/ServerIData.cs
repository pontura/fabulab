using System;
using UnityEngine;

namespace BoardItems.BoardData
{
    [Serializable]
    public class ServerIData
    {
        public int galleryID;
        public int part;
        public int id;

        public V3 position;
        public V3 rotation;
        public V3 scale;
        public AnimationsManager.anim anim;
        public PalettesManager.colorNames color;
    }

    public struct V3
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public V3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public V3(Vector3 v) {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }
    }
}
