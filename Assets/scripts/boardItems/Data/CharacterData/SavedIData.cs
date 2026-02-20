using UnityEngine;
using System;

namespace BoardItems.BoardData
{
    [Serializable]
    public class SavedIData
    {
        public int galleryID;
        public int part;
        public int id;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public AnimationsManager.anim anim;
        public PalettesManager.colorNames color;

        public ServerIData GetServerIData() {
            ServerIData sid = new ServerIData();
            sid.galleryID = galleryID;
            sid.id = id;
            sid.part = part;
            sid.position = new V3(position);
            sid.rotation = new V3(rotation);
            sid.scale = new V3(scale);
            sid.anim = anim;
            sid.color = color;
            return sid;
        }

        public void LoadServerData(ServerIData serverData) {
            id = serverData.id;
            galleryID = serverData.galleryID;
            part = serverData.part;
            position = new Vector3(serverData.position.x, serverData.position.y, serverData.position.z);
            rotation = new Vector3(serverData.rotation.x, serverData.rotation.y, serverData.rotation.z);
            scale = new Vector3(serverData.scale.x, serverData.scale.y, serverData.scale.z);
            anim = serverData.anim;
            color = serverData.color;
            Debug.Log(galleryID + ", " + part + ", " + position + ", " + rotation + ", " + scale + ", " + anim + ", " + color);
        }
    }
}