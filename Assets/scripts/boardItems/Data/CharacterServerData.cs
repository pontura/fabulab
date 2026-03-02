using System;
using System.Collections.Generic;

namespace BoardItems.BoardData
{
    [Serializable]
    public class SOPartServerData
    {
        public List<ServerIData> items;
    }

    [Serializable]
    public class CharacterServerData : SOPartServerData
    {
        public PalettesManager.colorNames armsColor;
        public PalettesManager.colorNames legsColor;
        public PalettesManager.colorNames eyebrowsColor;
        public List<string> creators;
    }
    [Serializable]
    public class SObjectServerData : SOPartServerData
    {
        public SObjectData.types type;
        public List<string> creators;
    }
}