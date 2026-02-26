using BoardItems;
using BoardItems.BoardData;
using System;
using System.Collections.Generic;
using UI.MainApp;
using UnityEngine;
using Yaguar.StoryMaker.DB;
using static UI.BoardUI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public GameObject backBtn;
        static UIManager mInstance = null;
        public BoardUI boardUI;
        public GallerySelectorUI gallerySelectorUI;
        public WorkDetailUI workDetailUI;
        public ZoomsManager zoomManager;
        CharacterEdition characterEdition;
        [SerializeField] ConfirmationScreen confirmationScreen;

        public enum screenType
        {
            Home,
            Create,
            Albums,
            Galleries,
            Creation_Character,
            WorkDetail,
            UserScreen,
            Creation_Objects,
            StoryMaker
        }

        public static UIManager Instance
        {
            get
            {
                return mInstance;
            }
        }
        List<screenType> backToScreen;
        void Awake()
        {
            confirmationScreen.Init();
            backToScreen = new List<screenType>();
            zoomManager = GetComponent<ZoomsManager>();
            characterEdition = GetComponent<CharacterEdition>();
            if (!mInstance)
                mInstance = this;
        }
        private void Start()
        {
            Events.ShowScreen += OnShowScreen;
            Init();
            Invoke("InitGalleryDelayed", 0.1f);
        }
        private void OnDestroy()
        {
            Events.ShowScreen -= OnShowScreen;
        }
        private void OnShowScreen(screenType type)
        {
            backToScreen.Add(type);
            switch(type)
            {
                case screenType.Home:
                    backBtn.SetActive(false);
                    break;
                default:
                    backBtn.SetActive(true);
                    break;
            }
        }
        void InitGalleryDelayed() // to-do inicia los items:
        {
            GaleriasData.GalleryData gd = Data.Instance.galeriasData.GetGallery(1);
           // InitGallery(gd, true, null);
            Events.InitGallery(gd, true, null);
        }
        void Init()
        {
            string uid = Data.Instance.userData.userDataInDatabase.uid;
            if (uid != "")
                Data.Instance.cacheData.GetUser(uid, OnUserDone);
            Home();
        }

        private void OnUserDone(CacheData.UserData uData)
        {
            print("OnUserDone UID " + uData);
        }

        public void Home()
        {
            Events.ShowScreen(UIManager.screenType.Home);
        }
        public void Create()
        {
            Events.ShowScreen(UIManager.screenType.Create);
        }
        public void CreateSelected(int id)
        {
            if (id == 1)
                NewStory(); // TO-DO
            else if (id == 2)
                NewCharacter();
            else
                NewObject();
        }
        public void Albums()
        {
            Events.ShowScreen(UIManager.screenType.Albums);
        }
        public void NewStory() {
            boardUI.SetEditingType(editingTypes.SCENE);
            Data.Instance.scenesData.StartNewStory("");
            boardUI.SetEditingType(editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);
        }
        public void NewCharacter()
        {
            boardUI.SetEditingType(editingTypes.CHARACTER);
            Data.Instance.charactersData.ResetCurrentID();
            Events.OnPresetReset();
            GaleriasData.GalleryData gd = Data.Instance.galeriasData.GetGallery(1);
            Events.InitGallery(gd, true, InitCharacterScreen);
            Events.EmptySceneItems();
        }
        void InitCharacterScreen()
        {
            Events.ShowScreen(UIManager.screenType.Creation_Character);
        }
        public void NewObject()
        {
            boardUI.SetEditingType(editingTypes.OBJECT);
            Events.OnCharacterReset();
            Events.OnPresetReset();
            GaleriasData.GalleryData gd = Data.Instance.galeriasData.GetGallery(1);
            Events.InitGallery(gd, true, InitObjectsScreen);
            Events.EmptySceneItems();
        }
        void InitObjectsScreen()
        {
            Events.ShowScreen(UIManager.screenType.Creation_Objects);
        }
        public void LoadWork(editingTypes type, string id)
        {
            boardUI.SetEditingType(type);
            boardUI.LoadWork(id);
            switch(type)
            {
                case editingTypes.CHARACTER:
                    Events.ShowScreen(UIManager.screenType.Creation_Character);
                    break;
                case editingTypes.OBJECT:
                    Events.ShowScreen(UIManager.screenType.Creation_Objects);
                    break;
            }
        }
        public void Back()
        {
            if (backToScreen[backToScreen.Count - 1] == screenType.Creation_Character && characterEdition.ChangesMade())
            {
                Events.OnConfirm("All changes will be lost", "Confirm and exit", "Cancel", ExitConfirmed);
            } else {
                SetBack();
            }
        }
        void SetBack()
        {
            if (backToScreen.Count < 3)
                Events.ShowScreen(screenType.Home);
            else
                Events.ShowScreen(backToScreen[backToScreen.Count - 2]);
            backToScreen.RemoveAt(backToScreen.Count - 1);
            backToScreen.RemoveAt(backToScreen.Count - 1);
        }
        void ExitConfirmed(bool exit)
        {
            if(exit)
                SetBack();
        }
        public void ShowWorkDetail(CharacterData wd)
        {
            Sprite sprite = Sprite.Create(wd.thumb, new Rect(0, 0, wd.thumb.width, wd.thumb.height), Vector2.zero);
            Events.ShowScreen(UIManager.screenType.WorkDetail);
            workDetailUI.ShowWorkDetail(wd.id, sprite, true);
            Events.ResetItems();
        }
    }

}