using BoardItems.BoardData;
using System;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllCharactersScreen : UserCharactersScreen
    {
        protected override void AddCharacterMetadata(CharacterMetaData cd) {
            if (!cd.isPublic)
                return;

            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            print("go " + go);
            if (StoryMakerEvents.isEditing)
                go.Init(cd, MetadataTypes.characters, Duplicate);
            else
                go.Init(cd, MetadataTypes.characters, OpenWork);
        }

        protected override void OnCharacterMetadataUpdated(CharacterMetaData fd) {
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                if (!fd.isPublic) {
                    Destroy(btn.gameObject);
                    Invoke(nameof(ResetAndSetScroll), Time.deltaTime * 3);
                } else {
                    if (StoryMakerEvents.isEditing)
                        btn.Init(fd, MetadataTypes.characters, Duplicate);
                    else
                        btn.Init(fd, MetadataTypes.characters, OpenWork);
                    //btn.transform.SetAsFirstSibling();
                    ResetAndSetScroll();
                }
            } else {
                if (fd.isPublic) {
                    AddCharacterMetadata(fd);
                    ResetAndSetScroll();
                }
            }
        }       

        protected override void LoadNext()
        {
            foreach(CharacterMetaData cd in Data.Instance.charactersData.charactersMetaData)
            {
                AddCharacterMetadata(cd);
            }

            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }
        public void Duplicate(string soID)
        {
            Events.DuplicateCharacter(soID);
        }                
    }

}