using System.Collections.Generic;
using BoardItems;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class GameSelector : MonoBehaviour
    {
        [SerializeField] ThumbButton btn;
        [SerializeField] Transform container;
        [SerializeField] AllStoriesScreen allStoriesScreen;
        GameData gameData;

        public void Show(bool isOn)
        {
            print("GameSelector Show " + isOn);
            gameObject.SetActive(isOn);
            gameData = Data.Instance.gamesManager.GetGame(Data.Instance.gamesManager.activaGameData);
            if(isOn)
            {
                Utils.RemoveAllChildsIn(container);
                foreach(GameIdEntry g in gameData.ids)
                {
                    FilmDataFabulab cd = Data.Instance.scenesData.GetMeta(g.id);
                    
                    ThumbButton i = Instantiate(btn, container);
                    i.Init(cd.id, OnClick);
                }
            }
        }
        void OnClick(string id)
        {
            allStoriesScreen.OpenWork(id);
        }
        public void OnStartPlaying()
        {
            print("OnStartPlaying idSelected: " + ScenesManager.Instance.currentFilmData.id);
            allStoriesScreen.OpenGame(ScenesManager.Instance.currentFilmData.id);
        }
    }
}