using System;
using System.Collections.Generic;

namespace BoardItems.BoardData
{
    [Serializable]
    public class SOData : CharacterPartData
    {

        public new CharacterServerData GetServerData()
        {
            CharacterServerData csd = new CharacterServerData();
            List<ServerIData> csdItems = new List<ServerIData>();
            foreach (SavedIData sid in items)
            {
                csdItems.Add(sid.GetServerIData());
            }
            csd.items = csdItems;
            return csd;
        }

        public void LoadServerData(CharacterServerData serverData)
        {
            items = new List<SavedIData>();
            foreach (ServerIData sid in serverData.items)
            {
                SavedIData savedData = new SavedIData();
                savedData.LoadServerData(sid);
                items.Add(savedData);
            }
        }
    }
}
