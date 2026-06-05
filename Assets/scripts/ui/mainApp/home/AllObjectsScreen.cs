using BoardItems.BoardData;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllObjectsScreen : UserObjectsScreen
    {
        protected override void AddPropMetadata(PropMetaData fd) {
            if (!fd.isPublic)
                return;

            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);

            go.Init(fd, MetadataTypes.so, OpenWork);
            //go.Init(fd, MetadataTypes.so);

        }

        protected override void OnPropMetadataUpdated(PropMetaData fd) {
            if (fd.type == type) {
                ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
                ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
                if (btn != null) {
                    if (!fd.isPublic) {
                        Destroy(btn.gameObject);
                    } else {
                        btn.Init(fd, MetadataTypes.so);
                        btn.transform.SetAsFirstSibling();
                    }
                } else {
                    if (fd.isPublic) {
                        AddPropMetadata(fd);
                    }
                }
            }
        }

        public override void SetType()
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