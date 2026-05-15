using BoardItems.BoardData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllObjectsFullScreenScreen : MonoBehaviour
    {
        public ItemSelectorBtn workBtn_prefab;
        public Transform container;
        SObjectData.types type;

        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
        }
        public void Init(SObjectData.types type)
        {
            this.type = type;
            Utils.RemoveAllChildsIn(container);

            List<PropMetaData> all;
            switch(type)
            {
                case SObjectData.types.generic:
                    Data.Instance.sObjectsData.Type = SObjectData.types.generic;
                    all = Data.Instance.sObjectsData.GetMetadataByType(SObjectData.types.generic);
                    break;
                case SObjectData.types.background:
                    Data.Instance.sObjectsData.Type = SObjectData.types.background;
                    all = Data.Instance.sObjectsData.GetMetadataByType(SObjectData.types.background);
                    break;
                default:
                    all = new List<PropMetaData>();
                    break;
            }
            foreach (PropMetaData cd in all)
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, container);
                print("go " + go);
                go.Init(cd);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }
        }
       
        public void OpenWork(string id)
        {
            if(StoryMakerEvents.isEditing)
            {
                switch (type)
                {
                    case SObjectData.types.generic:
                        Events.DuplicateSO(id);
                        break;
                    case SObjectData.types.background:
                        Events.DuplicateSOBG(id);
                        break;
                }
            }
            else
                UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
        }

    }

}