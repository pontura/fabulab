using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoardItems.BoardData
{
    [Serializable]
    public class CharacterPartData
    {
        public string id;
        public Texture2D thumb;
        public List<SavedIData> items;

        public Sprite GetSprite() {
            if (thumb == null) {
                Debug.LogError("No hay thumb para character id: " + id);
                return null;
            }
            return Sprite.Create(thumb, new Rect(0, 0, thumb.width, thumb.height), Vector2.zero);
        }

        public CharacterPartServerData GetServerData() {
            CharacterPartServerData cpsd = new CharacterPartServerData();
            List<ServerIData> csdItems = new List<ServerIData>();
            foreach (SavedIData sid in items) {
                csdItems.Add(sid.GetServerIData());
            }
            cpsd.items = csdItems;
            return cpsd;
        }

        public void LoadServerData(CharacterPartServerData serverData) {
            items = new List<SavedIData>();
            Debug.Log(serverData.items == null);
            Debug.Log(serverData.items.Count);
            foreach (ServerIData sid in serverData.items) {
                SavedIData savedData = new SavedIData();
                savedData.LoadServerData(sid);
                items.Add(savedData);
            }
        }
    }
}