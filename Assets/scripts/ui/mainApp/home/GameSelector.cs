using System.Collections.Generic;
using BoardItems;
using UnityEngine;

namespace UI.MainApp.Home.User
{
    public class GameSelector : MonoBehaviour
    {
        [SerializeField] ThumbButton btn;
        [SerializeField] Transform container;
        [SerializeField] AllStoriesScreen allStoriesScreen;
        GameData gameData;
        int idSelected;
        public void Show(bool isOn)
        {
            idSelected = 0;
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
            StIDSelected(id);
            allStoriesScreen.OpenWork(id);
        }
        void StIDSelected(string id)
        {
            idSelected = 0;
            foreach(GameIdEntry g in gameData.ids)
            {
                if(g.id == id)
                    return;
                idSelected++;
            }
        }
        public void OnStartPlaying()
        {
            print("OnStartPlaying idSelected: " + idSelected);
            allStoriesScreen.OpenGame(gameData.ids[idSelected].id);
        }
    }
}