using BoardItems;
using BoardItems.BoardData;
using Common.UI;
using System;
using System.Collections.Generic;
using UI.MainApp.Home.User;
using UnityEngine;
using Yaguar.Auth;
using System.Linq;

namespace UI.MainApp.Home
{
    public class UserScreen : MonoBehaviour {
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
            Events.OnCharacterMetadataUpdated += OnCharacterMetadataUpdated;
            Events.OnCharacterMetadataRemoved += OnCharacterMetadataRemoved;
            Events.OnPropMetadataUpdated += OnPropMetadataUpdated;
            Events.OnPropMetadataRemoved += OnPropMetadataRemoved;
            Events.OnFilmMetadataUpdated += OnFilmMetadataUpdated;
            Events.OnFilmMetadataRemoved += OnFilmMetadataRemoved;
            FirebaseAuthManager.Instance.OnSignedOut += OnSignedOut;
        }

        private void OnChangeName(string username) {
            usernameField.text = username;
        }
        public void Create() {
            int screen = tabActive;
            if (tabActive > 1)
                UIManager.Instance.Create();
            else
                UIManager.Instance.CreateSelected(tabActive + 1);
        }
        private void OnDestroy() {
            Events.ChangeName -= OnChangeName;
            Events.OnCharacterMetadataUpdated -= OnCharacterMetadataUpdated;
            Events.OnCharacterMetadataRemoved -= OnCharacterMetadataRemoved;
            Events.OnPropMetadataUpdated -= OnPropMetadataUpdated;
            Events.OnPropMetadataRemoved -= OnPropMetadataRemoved;
            Events.OnFilmMetadataUpdated -= OnFilmMetadataUpdated;
            Events.OnFilmMetadataRemoved -= OnFilmMetadataRemoved;
            FirebaseAuthManager.Instance.OnSignedOut -= OnSignedOut;
        }

        void OnSignedOut() {
            firstTime = true;
        }

        public void Show(bool isOn) {
            gameObject.SetActive(isOn);
            print("userData Sho hambuguerMenu " + isOn);
            if (isOn) {
                AudioManager.Instance.musicManager.Play("board");
                userDataScreen.Show(true);
                profilePicture.InitOwner();
                string username = Data.Instance.userData.userDataInDatabase.username;
                OnChangeName(username);
                SetPublicFields();
                hamburguerOn = false;
                hambuguerMenu.SetActive(false);
            }
            if (isOn && firstTime) {
                firstTime = false;
                tabs.Init(OnTabClicked);
                List<string> tabNames = new List<string>() { "Historias", "Personajes", "Objetos", "Info" };
                tabs.SetTabNames(tabNames);
            } else {
                tabs.ReOpen();
            }
        }
        int tabActive;
        void OnTabClicked(int id) {
            this.tabActive = id;
            print("OnTabClicked " + id + " name: " + gameObject.name);

            charactersScreen.Show(false);

            storiesScreen.Show(false);
            objects.Show(false);

            switch (id) {

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
        public void OnBoardingBack() {
            UIManager.Instance.onboardingManager.Reset();
        }
        public void ToggleHamburguer() {
            print("ToggleHamburguer " + hamburguerOn);
            hamburguerOn = !hamburguerOn;
            hambuguerMenu.SetActive(hamburguerOn);
        }
        void SetPublicFields() {
            publicStoriesField.text = "" + Data.Instance.scenesData.userFilmsData.Count(x => x.isPublic);
            publicChField.text = "" + Data.Instance.charactersData.userCharactersMetaData.Count(x => x.isPublic);
            publicObjField.text = "" + Data.Instance.sObjectsData.userMetaData.Count(x => x.isPublic);
        }

        void OnCharacterMetadataUpdated(CharacterMetaData fd) {
            Debug.Log("# OnCharacterMetadataUpdated");
            publicChField.text = "" + Data.Instance.scenesData.userFilmsData.Count(x => x.isPublic);
        }
        void OnCharacterMetadataRemoved(string id) {
            Debug.Log("# OnCharacterMetadataUpdated");
            publicChField.text = "" + Data.Instance.scenesData.userFilmsData.Count(x => x.isPublic);
        }
        void OnPropMetadataUpdated(CharacterMetaData fd) {
            Debug.Log("# OnPropMetadataUpdated");
            publicObjField.text = "" + Data.Instance.sObjectsData.userMetaData.Count(x => x.isPublic);
        }

        void OnPropMetadataRemoved(string id) {
            Debug.Log("# OnPropMetadataRemoved");
            publicObjField.text = "" + Data.Instance.sObjectsData.userMetaData.Count(x => x.isPublic);
        }
        void OnFilmMetadataUpdated(FilmDataFabulab fd) {
            Debug.Log("# OnFilmMetadataUpdated");
            publicStoriesField.text = "" + Data.Instance.scenesData.userFilmsData.Count(x => x.isPublic);
        }

        void OnFilmMetadataRemoved(string id) {
            Debug.Log("# OnFilmMetadataRemoved");
            publicStoriesField.text = "" + Data.Instance.scenesData.userFilmsData.Count(x => x.isPublic);
        }
    }
}
