using System.Collections.Generic;
using BoardItems;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class GameSelector : MonoBehaviour
    {
        [SerializeField] ThumbButton btn;
        [SerializeField] List<ThumbButton> buttons;
        [SerializeField] Transform container;
        [SerializeField] AllStoriesScreen allGameStoriesScreen;
        GameData gameData;

        public void Show(bool isOn)
        {
            buttons = new List<ThumbButton>();
            print("GameSelector Show " + isOn);
            gameObject.SetActive(isOn);
            gameData = Data.Instance.gamesManager.GetGame(Data.Instance.gamesManager.activaGameData);
            if(isOn)
            {
                allGameStoriesScreen.gameObject.SetActive(false);
                Utils.RemoveAllChildsIn(container);
                foreach(GameIdEntry g in gameData.ids)
                {
                    FilmDataFabulab cd = Data.Instance.scenesData.GetMeta(g.id);
                    
                    ThumbButton i = Instantiate(btn, container);
                    buttons.Add(i);
                    i.Init(cd.id, OnClick);
                }
                SetSelected(gameData.ids[0].id);
            }
        }
        void SetSelected(string id)
        {
            int i = 0;
            foreach(GameIdEntry g in gameData.ids)
            {                  
                print(id  + "__________________" + g.id);
                buttons[i].SetSelected(g.id ==ScenesManager.Instance.currentFDataID);
                i++;
            }
        }
        void OnClick(string id)
        {
            allGameStoriesScreen.OpenWork(id);
            SetSelected(id);
        }
        public void OnStartPlaying()
        {
            print("OnStartPlaying idSelected: " + ScenesManager.Instance.currentFilmData.id);
            allGameStoriesScreen.OpenGame(ScenesManager.Instance.currentFilmData.id);
        }
        public void OpenGameStories()
        {
            UIManager.Instance.AddBackTo(UIManager.screenType.GamesStories, true);
            allGameStoriesScreen.gameObject.SetActive(true);
            allGameStoriesScreen.Show(true);
        }
    }
}