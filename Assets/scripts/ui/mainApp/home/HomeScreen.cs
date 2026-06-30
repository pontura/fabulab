using Common.UI;
using System.Collections.Generic;
using UI.MainApp.Home.User;
using UnityEngine;

namespace UI.MainApp.Home
{
    public class HomeScreen : MonoBehaviour
    {
        [SerializeField] TabController tabs;
        [SerializeField] AllStoriesScreen stories;
        [SerializeField] AllCharactersScreen charactersScreen;
        [SerializeField] AllObjectsScreen objects;
        
        [SerializeField] TMPro.TMP_Text usernameField;

        [SerializeField] ProfilePicture profilePicture;
        bool firstTime = true;
        private void Start() {
            Events.OnAllFilmMetadataLoadDone += OnServerDataLoadDone;
            Events.ChangeName += OnChangeName;
        }
        public void Create()
        {            
            
            int screen = tabActive;
            if(tabActive>1) 
                UIManager.Instance.Create();
            else
                UIManager.Instance.CreateSelected(tabActive+1);
        }
        private void OnChangeName(string username)
        {
            usernameField.text = username;
        }

        private void OnDestroy() {
            Events.ChangeName -= OnChangeName;
            Events.OnAllFilmMetadataLoadDone -= OnServerDataLoadDone;
        }

        void OnServerDataLoadDone() {
            tabs.Init(OnTabClicked);
        }

        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn && firstTime)
            {
                AudioManager.Instance.musicManager.Play("main");
                firstTime = false;
                tabs.Init(OnTabClicked);
                 List<string> tabNames = new List<string>() { "Historias", "Personajes", "Objetos" };
                tabs.SetTabNames(tabNames);
                profilePicture.InitOwner();
                string username = Data.Instance.userData.userDataInDatabase.username;
                OnChangeName(username);
            }else
            {
                if(tabs.lastTabClicked.id == 3) // si el ultimo tab fue el user, fuerza a historias:
                {
                    OnTabClicked(0);
                    tabs.SetActive(0);
                } else
                    tabs.ReOpen();                
            }
        }
        int tabActive;
        void OnTabClicked(int id)
        {
            this.tabActive = id;
            print("On home TabClicked " + id);

            charactersScreen.Show(false);
            objects.Show(false);
            stories.Show(false);

            switch (id)
            {
                case 0:
                    AudioManager.Instance.uiSfxManager.PlayTransp("click", -3);
                    stories.Show(true);
                    break;
                case 1:
                    AudioManager.Instance.uiSfxManager.Play("click");
                    charactersScreen.Show(true);
                    break;
                case 2:
                    AudioManager.Instance.uiSfxManager.PlayTransp("click", 2);
                    objects.Show(true);
                    break;
                case 3: // USER                    
                    Events.ShowScreen(UIManager.screenType.UserScreen);
                    break;
            }
        }
    }
}
