using BoardItems;
using BoardItems.BoardData;
using Common.UI;
using System.Collections.Generic;
using UI.MainApp.Home.User;
using UnityEditor;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.StoryMaker.Editor;

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
        [SerializeField] GameObject hambuguerMenu;
        
        [SerializeField] TMPro.TMP_Text publicStoriesField;
        [SerializeField] TMPro.TMP_Text publicChField;
        [SerializeField] TMPro.TMP_Text publicObjField;

        bool hamburguerOn;
        bool firstTime = true;
        private void Start() {
            Events.ChangeName += OnChangeName;
            FirebaseAuthManager.Instance.OnSignedOut += OnSignedOut;
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
            FirebaseAuthManager.Instance.OnSignedOut -= OnSignedOut;
        }

        void OnSignedOut() {
            firstTime = true;
        }

        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn && firstTime)
            {
                hamburguerOn = false;
                hambuguerMenu.SetActive(false);
                AudioManager.Instance.musicManager.Play("board");
                firstTime = false;
                profilePicture.InitOwner();
                tabs.Init(OnTabClicked);
                List<string> tabNames = new List<string>() { "Historias", "Personajes", "Objetos", "Info" };
                tabs.SetTabNames(tabNames);
                string username = Data.Instance.userData.userDataInDatabase.username;
                OnChangeName(username);
                SetPublicFields();
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
            }
        }
        public void OnBoardingBack()
        {
            UIManager.Instance.onboardingManager.Reset();
        } 
        public void ToggleHamburguer()
        {
            hamburguerOn = !hamburguerOn;
            hambuguerMenu.SetActive(hamburguerOn);
        }
        void SetPublicFields()
        {
            int publicStories = 0;
            int publicCharacters = 0;
            int publicObjects = 0;

            List<FilmDataFabulab> all_fd = Data.Instance.scenesData.userFilmsData;
            foreach(FilmDataFabulab f in all_fd)
            {
                if(f.isPublic)
                    publicStories++;
            }
            List<CharacterMetaData> cll_ch = Data.Instance.charactersData.userCharactersMetaData;
            foreach(CharacterMetaData c in cll_ch)
            {
                if(c.isPublic)
                    publicCharacters++;
            }
             List<PropMetaData> all_obj = Data.Instance.sObjectsData.userMetaData;
            foreach(PropMetaData c in all_obj)
            {
                if(c.isPublic)
                    publicObjects++;
            }

            publicStoriesField.text = publicStories.ToString();
            publicChField.text = publicCharacters.ToString();
            publicObjField.text = publicObjects.ToString();
        }
    }
}
