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
}
