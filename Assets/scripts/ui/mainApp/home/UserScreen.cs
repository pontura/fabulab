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

        bool firstTime = true;
        private void Start() {
            Events.ChangeName += OnChangeName;
        }

        private void OnChangeName(string username)
        {
            usernameField.text = username;
        }
        public void Create()
        {    
            int screen = tabActive;
            if(tabActive>1) 
                UIManager.Instance.Create();
            else
                UIManager.Instance.CreateSelected(tabActive+1);
        }
        private void OnDestroy() {
            Events.ChangeName -= OnChangeName;
        }
        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn && firstTime)
            {
                AudioManager.Instance.musicManager.Play("board");
                firstTime = false;
                profilePicture.InitOwner();
                tabs.Init(OnTabClicked);
                List<string> tabNames = new List<string>() { "Historias", "Personajes", "Objetos", "Info" };
                tabs.SetTabNames(tabNames);
                string username = Data.Instance.userData.userDataInDatabase.username;
                OnChangeName(username);
            }
            else
            {
                tabs.ReOpen();
            }
        }
        int tabActive;
        void OnTabClicked(int id)
        {
            this.tabActive = id;
            print("OnTabClicked " + id + " name: "  +gameObject.name);

            userDataScreen.Show(false);
            charactersScreen.Show(false);

            storiesScreen.Show(false);
            objects.Show(false);

            switch (id)
            {
              
                case 0:
                    AudioManager.Instance.uiSfxManager.PlayTransp("click", 5);
                    storiesScreen.Show(true);
                    break;
                case 1:
                    AudioManager.Instance.uiSfxManager.PlayTransp("click", 2);
                    charactersScreen.Show(true);
                    break;
                case 2:
                    AudioManager.Instance.uiSfxManager.Play("click");
                    objects.Show(true);
                    break;
                case 3:
                    AudioManager.Instance.uiSfxManager.PlayTransp("click", 2);
                    userDataScreen.Show(true);
                    break;
            }
        }
        public void OnBoardingBack()
        {
            UIManager.Instance.onboardingManager.Reset();
        } 
    }
}
