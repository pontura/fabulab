using BoardItems;
using Firebase.Analytics;
using System;
using System.Collections.Generic;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllStoriesScreen : UserStoriesScreen
    {       
        string id;
        bool isGame;

        public void OnEnable()
        {
            Data.Instance.gamesManager.SetPlaying(false);
            Data.Instance.gamesManager.OnSetActiveGame("");
        }

        protected override void Init()
        {
            isGame = false;

              if (firstLoadDone)
                return;

            if(Data.Instance.scenesData.filmsData.Count > 0) {
                firstLoadDone = true;

                foreach (Transform child in worksContainer) {
                    if (child.tag != "Persistent")
                        Destroy(child.gameObject);
                }
                
                TitleLine t = Instantiate(titleLine, worksContainer);
                t.Init("JUEGOS");
                List<GameData>  all = Data.Instance.gamesManager.GetGamesBySection("stories");
                foreach(GameData gd in all)
                {
                    GameButton gb = Instantiate(gameButton, worksContainer);
                    gb.Init(gd, OnGameClicked);
                }

                t = Instantiate(titleLine, worksContainer);
                t.Init("Últimas Historias");
                LoadNext();
            }
        }
        private void OnGameClicked(GameData gameData)
        {
            string storyId = gameData.ids[0].id;
            Data.Instance.gamesManager.OnSetActiveGame(gameData.id);
            OpenWork(storyId);
        }

        protected override void LoadNext()
        {
            Debug.Log("% AllStoriesScreen LoadNext");
            foreach(FilmDataFabulab cd in Data.Instance.scenesData.filmsData)
            {
                AddFilmMetadata(cd);
            }

            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }

        protected override void OnLoadedDone() {
            base.OnLoadedDone();
            AudioManager.Instance.musicManager.Play("intro");
        }

        protected override void AddFilmMetadata(FilmDataFabulab fd) {
            if (!fd.isPublic)
                return;
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            go.Init(fd.id, null);
            go.GetComponent<ItemSelectorStory>().SetContent(fd, this, false);
        }

        protected override void OnFilmMetadataUpdated(FilmDataFabulab fd) {
            Debug.Log("% AllStoriesScreen OnFilmMetadataUpdated " + gameObject.name);
            ItemSelectorStory[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorStory>();
            ItemSelectorStory btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                if (!fd.isPublic) {
                    Destroy(btn.gameObject);
                    Invoke(nameof(ResetAndSetScroll),Time.deltaTime * 3);
                } else {
                    btn.Init(fd.id,null);
                    btn.SetContent(fd, this, false);
                    //btn.transform.SetAsFirstSibling();
                    ResetAndSetScroll();
                }
            } else {
                if (fd.isPublic) {
                    AddFilmMetadata(fd);
                    ResetAndSetScroll();
                }
            }
        }
        public void OpenGame(string id)
        {
            isGame = true;
            Data.Instance.gamesManager.SetPlaying(true);
            string newID = Data.Instance.gamesManager.CheckIfStoryWasMade();
            OpenWork(newID);
        }
        public override void OpenWork(string id) {
            
            print("OpenWork " + id + " isGame_: " + isGame);
            this.id = id;
            Events.OnLoadingParent(null, LoadingDone);
        }
       
        void LoadingDone()
        {
            Events.OnLoading(true);
            UIManager.Instance.boardUI.SetEditingType(BoardUI.editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            Data.Instance.scenesData.LoadFilm(id);
            Invoke(nameof(SetStoryEditionState), Time.deltaTime * 2);
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "others_story_opened",
                new Parameter("story_id", id)                
            );
        }
        
        void SetStoryEditionState() {
            StoryMakerEvents.EnableStoryEdition(Data.Instance.gamesManager.IsEditing());
            StoryMakerEvents.EnableInputManager(Data.Instance.gamesManager.IsEditing());
        }        
    }
}