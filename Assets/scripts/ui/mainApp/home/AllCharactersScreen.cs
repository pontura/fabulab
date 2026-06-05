using BoardItems.BoardData;
using System;
using UnityEngine;
using UnityEngine.UI;
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
                } else {
                    btn.Init(fd.GetSprite());
                    btn.transform.SetAsFirstSibling();
                }
            } else {
                if (fd.isPublic) {
                    AddCharacterMetadata(fd);
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