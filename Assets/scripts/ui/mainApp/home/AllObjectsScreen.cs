using BoardItems.BoardData;
using System.Collections.Generic;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllObjectsScreen : UserObjectsScreen
    {
        public override void OnLoadData()
        {
            switch (type)
            {
                default:
                    Data.Instance.sObjectsData.Type = SObjectData.types.generic;
                    all = Data.Instance.sObjectsData.GetMetadataByType(SObjectData.types.generic);
                    break;
                case SObjectData.types.background:
                    Data.Instance.sObjectsData.Type = SObjectData.types.background;
                    all = Data.Instance.sObjectsData.GetMetadataByType(SObjectData.types.background);
                    break;
            }
        }
        public override void InitTabs()
        {
            if (buttons.Length == 0)
            {
                foreach (Button b in buttons)
                {
                    b.gameObject.SetActive(false);
                }
            }
            else
            {
                base.InitTabs();
            }
        }
        public override void OpenWork(string id)
        {
            if(StoryMakerEvents.isEditing)
                Events.DuplicateSO(id);
            else
            {
                base.OpenWork(id);
            }
        }        
    }

}