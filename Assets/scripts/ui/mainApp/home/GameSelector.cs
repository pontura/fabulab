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
        [SerializeField] AllStoriesScreen allStoriesScreen;
        GameData gameData;

        public void Show(bool isOn)
        {
            buttons = new List<ThumbButton>();
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
            allStoriesScreen.OpenWork(id);
            SetSelected(id);
        }
        public void OnStartPlaying()
        {
            print("OnStartPlaying idSelected: " + ScenesManager.Instance.currentFilmData.id);
            allStoriesScreen.OpenGame(ScenesManager.Instance.currentFilmData.id);
        }
    }
}