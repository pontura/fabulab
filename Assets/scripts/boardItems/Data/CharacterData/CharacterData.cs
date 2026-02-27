using System;
using System.Collections.Generic;

namespace BoardItems.BoardData
{

    [Serializable]
    public class CharacterData : CharacterPartData
    {
        public PalettesManager.colorNames armsColor;
        public PalettesManager.colorNames legsColor;
        public PalettesManager.colorNames eyebrowsColor;

        public new CharacterServerData GetServerData() {
            CharacterServerData csd = new CharacterServerData();
            List<ServerIData> csdItems = new List<ServerIData>();
            foreach (SavedIData sid in items) {
                csdItems.Add(sid.GetServerIData());
            }
            csd.items = csdItems;
            csd.armsColor = armsColor;
            csd.legsColor = legsColor;
            csd.eyebrowsColor = eyebrowsColor;
            return csd;
        }

        public void LoadServerData(CharacterServerData serverData) {
            items = new List<SavedIData>();
            foreach (ServerIData sid in serverData.items) {
                SavedIData savedData = new SavedIData();
                savedData.LoadServerData(sid);
                items.Add(savedData);
            }
            armsColor = serverData.armsColor;
            legsColor = serverData.legsColor;
            eyebrowsColor = serverData.eyebrowsColor;
        }
       
    }
}
