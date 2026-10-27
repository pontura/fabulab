using BoardItems.BoardData;
using System;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class CharactersGameScreen : UserCharactersScreen
    {
        // Only the characters of the game (with its tag) are listed
        bool IsGameCharacter(CharacterMetaData cd) {
            return cd.tags != null && cd.tags.Contains(Data.Instance.charactersData.gameTag);
        }

        protected override ItemSelectorBtn AddCharacterMetadata(CharacterMetaData cd)
        {
            if (!IsGameCharacter(cd))
                return null;
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            go.Init(cd, MetadataTypes.characters, OpenWork);
            return go;
        }

        protected override void OnCharacterMetadataUpdated(CharacterMetaData fd) {
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                if (!fd.isPublic || !IsGameCharacter(fd)) {
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
                if (fd.isPublic && AddCharacterMetadata(fd) != null)
                    ResetAndSetScroll();
            }
        }

        protected override void LoadNext()
        {
            foreach(CharacterMetaData cd in Data.Instance.charactersData.charactersMetaData)
                AddCharacterMetadata(cd);

            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }

        
        public void Duplicate(string soID)
        {
            Events.DuplicateCharacter(soID);
        }                
    }

}