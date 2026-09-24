using BoardItems;
using System;
using UnityEngine;
namespace UI.MainApp.Home.User
{
    public class GamesStories : AllStoriesScreen
    {
        string loadedGameId;

        // a diferencia de AllStoriesScreen, esta pantalla depende del juego activo (lo setea GameSelector), asi que no lo reseteamos al activarse
        new void OnEnable()
        {
            Data.Instance.gamesManager.SetPlaying(false);
        }

        protected override void Init()
        {
            isGame = false;
            // la carga la maneja Show(), para poder recargar la lista cuando cambia el juego activo
        }
        public void ShowFromHome(bool isOn)
        {
            gameObject.SetActive(isOn);
        }
        public override void Show(bool isOn)
        {
            base.Show(isOn);
            if (!isOn || Data.Instance.scenesData.filmsData.Count == 0)
                return;

            string activeGameId = Data.Instance.gamesManager.activaGameData;
            if (firstLoadDone && loadedGameId == activeGameId)
                return;

            firstLoadDone = true;
            loadedGameId = activeGameId;

            foreach (Transform child in worksContainer) {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }
            LoadNext();
        }
        protected override void LoadNext()
        {
            GameData gd = Data.Instance.gamesManager.GetGame(Data.Instance.gamesManager.activaGameData);
            int gameId = 1;
            if(gd != null)
            {
                foreach(GameIdEntry gameIdEntry in gd.ids)
                {
                    TitleLine t = Instantiate(titleLine, worksContainer);
                    t.Init("Juego " + gameId);
                    foreach(string storyIds in gameIdEntry.storyIds)
                    {
                        Debug.Log("% Game Story id: " + storyIds);
                        FilmDataFabulab cd = Data.Instance.scenesData.GetMeta(storyIds);
                        AddFilmMetadata(cd);
                    }
                    gameId++;
                }
            }
            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }
         protected override void OnLoadedDone() {
            base.OnLoadedDone();
            UIManager.Instance.AddBackTo(UIManager.screenType.GamesStories, true);
        }

        //hacemos una nueva versi�n que no herede para que no se agreguen en tiempo real historias porque habr�a que filtrar y mostrarlas debajo del t�tulo correcto
        new void AddFilmMetadata(FilmDataFabulab fd) {
            if(fd == null) 
            {
                Debug.LogError("AddFilmMetadata fd is null");
                return;
            }
            if (fd.tags != null && fd.tags.Contains("games")){
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                go.Init(fd.id, null);
                go.GetComponent<ItemSelectorStory>().SetContent(fd, this, false);
            }
        }

        protected override void OnFilmMetadataAdded(FilmDataFabulab fd) {
            Debug.Log("% GamesStories OnFilmMetadataAdded");
            AddFilmMetadata(fd);
            if (fd!=null && fd.tags != null && fd.tags.Contains("games")) {
                int titleIndex = Data.Instance.gamesManager.GetIndexStory(fd.id);
                var childrenWithComponent = GetComponentsInChildren<TitleLine>(true);
                if (childrenWithComponent.Length == 0 && titleIndex<0) {
                    Debug.LogWarning("Not TitleLine class");
                    worksContainer.GetChild(worksContainer.childCount - 1).SetAsFirstSibling();
                } else {
                    Transform lastAdded = worksContainer.GetChild(worksContainer.childCount - 1);
                    int index = childrenWithComponent[titleIndex].transform.GetSiblingIndex();
                    lastAdded.SetSiblingIndex(index + 1);
                }
            }
            if (firstImageCache) {
                ResetCache();
            }
        }

        protected override void OnFilmMetadataUpdated(FilmDataFabulab fd) {
            Debug.Log("% GamesStories OnFilmMetadataUpdated " + gameObject.name);
            ItemSelectorStory[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorStory>();
            ItemSelectorStory btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {                
                btn.Init(fd.id, null);
                btn.SetContent(fd, this, false);
                //btn.transform.SetAsFirstSibling();
                ResetAndSetScroll();
            } else {
                OnFilmMetadataAdded(fd);
                ResetAndSetScroll();
            }
        }
        public override void OpenWork(string id) 
        {
            ShowFromHome(false);
            this.id = id;
            Events.OnLoadingParent(null, LoadingDone);
        }
        public override void SetStoryEditionState()
        {            
            base.SetStoryEditionState();
            if(Data.Instance.gamesManager.playing)
                UIManager.Instance.AddBackTo(UIManager.screenType.GameStoriesCreator, true);
        }
        public void BackToPlay()
        {
            ShowFromHome(false);
            GameData gs = Data.Instance.gamesManager.GetGame(Data.Instance.gamesManager.activaGameData);
            if (gs == null)
                return;
            string storyId = gs.ids[0].id;
            Data.Instance.gamesManager.OnSetActiveGame(gs.id);
            OpenWork(storyId);
        }
    }
}
