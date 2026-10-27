using BoardItems.BoardData;
using System;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllCharactersScreen : UserCharactersScreen
    {

        protected override void Start()
        {
            base.Start();
            GameData gameData = new GameData();
            gameData.title = "Hacé tu Superheroe";
            gameData.description = "¿Querés participar del concurso de superheroes?";
            if(gameButton != null)
                gameButton.Init(gameData, OnGameClicked);
        }

        private void OnGameClicked(GameData data)
        {
            Data.Instance.charactersData.Game = true;
            string hardcodedCharacterID = "-P-vL3PW-6wXaldES5wD";
            UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, hardcodedCharacterID);  
        }

        protected override ItemSelectorBtn AddCharacterMetadata(CharacterMetaData cd) {
            if (!cd.isPublic)
                return null;

            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            if (StoryMakerEvents.isEditing)
                go.Init(cd, MetadataTypes.characters, Duplicate);
            else
                go.Init(cd, MetadataTypes.characters, OpenWork);
            return go;
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