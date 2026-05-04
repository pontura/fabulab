using BoardItems.BoardData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllObjectsScreen : UserObjectsScreen
    {
        public override void Init()
        {
            Events.OnLoading(true,  LoadingType.Home);

            Utils.RemoveAllChildsIn(backgroundsContainer);
            Utils.RemoveAllChildsIn(objectsContainer);

            List<PropMetaData> generics = Data.Instance.sObjectsData.GetMetadataByType(SObjectData.types.generic);
            List<PropMetaData> backgrounds  = Data.Instance.sObjectsData.GetMetadataByType(SObjectData.types.background);

            AddTitle(0, "Objectos (" + generics.Count + ")");
            foreach (PropMetaData cd in generics)
            {
                AddPropMetadata(cd);
            }
            AddTitle(1, "Fondos (" + backgrounds.Count + ")");
            foreach (PropMetaData cd in backgrounds)
            {
                AddPropMetadata(cd);
            }

            if (Data.Instance.sObjectsData.metaData.Count > 0)
                firstLoad = true;

            Events.OnLoading(false, UI.LoadingType.Home);
        }
        
        public override void OpenWork(string id)
        {
            if(StoryMakerEvents.isEditing)
                Events.DuplicateSO(id);
            else
                UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
        }        
    }

}