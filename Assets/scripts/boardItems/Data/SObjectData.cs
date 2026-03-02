using System;
using System.Collections.Generic;

namespace BoardItems.BoardData
{

    [Serializable]
    public class SObjectData : SOPartData
    {
        public types type;

        public enum types
        {
            generic,
            background
        }

        public new SObjectServerData GetServerData()
        {
            SObjectServerData csd = new SObjectServerData();
            List<ServerIData> csdItems = new List<ServerIData>();
            foreach (SavedIData sid in items)
            {
                csdItems.Add(sid.GetServerIData());
            }
            csd.type = type;
            csd.items = csdItems;
            return csd;
        }

        public void LoadServerData(SObjectServerData serverData)
        {
            items = new List<SavedIData>();
            foreach (ServerIData sid in serverData.items)
            {
                SavedIData savedData = new SavedIData();
                savedData.LoadServerData(sid);
                items.Add(savedData);
            }
            type = serverData.type;
        }

    }
}
