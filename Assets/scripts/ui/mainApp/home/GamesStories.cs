using System.Collections.Generic;
using BoardItems;
using UnityEngine;
namespace UI.MainApp.Home.User
{
    public class GamesStories : AllStoriesScreen
    {
        protected override void Init()
        {
            isGame = false;

              if (firstLoadDone)
                return;

            if(Data.Instance.scenesData.filmsData.Count > 0) {
                firstLoadDone = true;
                
                LoadNext();
            }            
        }
        public void ShowFromHome(bool isOn)
        {
            gameObject.SetActive(isOn);
        }
        protected override void LoadNext()
        {
            Data.Instance.gamesManager.watchingFilmsMade = true;
            List<GameData>  all = Data.Instance.gamesManager.GetGamesBySection("stories");
            int gameId = 1;
            foreach(GameData gd in all)
            {
                foreach(GameIdEntry gameIdEntry in gd.ids)
                {
                    TitleLine t = Instantiate(titleLine, worksContainer);
                    t.Init("Finales historia " + gameId);
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
        protected override void AddFilmMetadata(FilmDataFabulab fd) {
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            go.Init(fd.id, null);
            go.GetComponent<ItemSelectorStory>().SetContent(fd, this, false);
        }
        public override void OpenWork(string id) {
            
            ShowFromHome(false);
            this.id = id;
            Events.OnLoadingParent(null, LoadingDone);
        }
        public void BackToPlay()
        {
            ShowFromHome(false);
            List<GameData>  all = Data.Instance.gamesManager.GetGamesBySection("stories");
            GameData gs =  all[0]; // TO-DO ahora siempre va al unico juego que hay:
            string storyId = gs.ids[0].id;
            Data.Instance.gamesManager.OnSetActiveGame(gs.id);
            OpenWork(storyId);
        }
    }
}
