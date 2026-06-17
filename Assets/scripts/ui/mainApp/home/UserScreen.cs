using Common.UI;
using System.Collections.Generic;
using UI.MainApp.Home.User;
using UnityEngine;

namespace UI.MainApp.Home
{
    public class UserScreen : MonoBehaviour
    {
        [SerializeField] TabController tabs;
        [SerializeField] UserDataScreen userDataScreen;
        [SerializeField] UserStoriesScreen storiesScreen;
        [SerializeField] UserCharactersScreen charactersScreen;
        [SerializeField] UserObjectsScreen objects;
        [SerializeField] ProfilePicture profilePicture;
        [SerializeField] TMPro.TMP_Text usernameField;

        
        private void Start() {
            Events.ChangeName += OnChangeName;
        }

        private void OnChangeName(string username)
        {
            usernameField.text = username;
        }

        private void OnDestroy() {
            Events.ChangeName -= OnChangeName;
        }
        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn)
            {
                profilePicture.InitOwner();
                tabs.Init(OnTabClicked);
                List<string> tabNames = new List<string>() { "Historias", "Personajes", "Objetos", "Info" };
                tabs.SetTabNames(tabNames);
                string username = Data.Instance.userData.userDataInDatabase.username;
                OnChangeName(username);
            }
        }
        void OnTabClicked(int id)
        {
            print("OnTabClicked " + id + " name: "  +gameObject.name);

            userDataScreen.Show(false);
            charactersScreen.Show(false);

            storiesScreen.Show(false);
            objects.Show(false);

            switch (id)
            {
              
                case 0:
                    storiesScreen.Show(true);
                    break;
                case 1:
                    charactersScreen.Show(true);
                    break;
                case 2:
                    objects.Show(true);
                    break;
                case 3:
                    userDataScreen.Show(true);
                    break;
            }
        }
    }
}
