using System;
using System.Collections.Generic;

namespace BoardItems.BoardData
{
    [Serializable]
    public class CharacterPartServerData
    {
        public List<ServerIData> items;
    }

    [Serializable]
    public class CharacterServerData : CharacterPartServerData
    {
        public PalettesManager.colorNames armsColor;
        public PalettesManager.colorNames legsColor;
        public PalettesManager.colorNames eyebrowsColor;
        public List<string> creators;
    }
}