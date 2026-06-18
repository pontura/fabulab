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
            if (isOn)
            {
                tabs.Init(OnTabClicked);
                int charactersQty = Data.Instance.charactersData.userCharacters.Count;
                List<string> tabNames = new List<string>() { "Historias", "Personajes", "Objetos" };
                tabs.SetTabNames(tabNames);
                profilePicture.InitOwner();
                string username = Data.Instance.userData.userDataInDatabase.username;
                OnChangeName(username);
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
                    stories.Show(true);
                    break;
                case 1:
                    charactersScreen.Show(true);
                    break;
                case 2:
                    objects.Show(true);
                    break;
                case 3: // USER
                    Events.ShowScreen(UIManager.screenType.UserScreen);
                    break;
            }
        }
    }
}
