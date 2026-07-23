using BoardItems;
using BoardItems.BoardData;
using BoardItems.Characters;
using Firebase.Analytics;
using OnBoarding;
using System.Collections;
using System.Collections.Generic;
using UI.MainApp;
using UI.MainApp.Home;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.StoryMaker.Editor;
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
        public UndoManager undoManager;
        public InputManager inputManager;
        public InfoDataScreen infoDataScreen;
        public OnBoardingManager onboardingManager;
        public HomePage homePage;

        public bool hasUnsavedChanges;

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
        [SerializeField] List<screenType> backToScreen;
        void Awake()
        {
            homePage = GetComponent<HomePage>();
            infoDataScreen = GetComponent<InfoDataScreen>();
            confirmationScreen.Init();
            backToScreen = new List<screenType>();
            zoomManager = GetComponent<ZoomsManager>();
            characterEdition = GetComponent<CharacterEdition>();
            undoManager = GetComponent<UndoManager>();
            inputManager = GetComponent<InputManager>();
            onboardingManager = GetComponent<OnBoardingManager>();
            if (!mInstance)
                mInstance = this;
        }
        private void Start()
        {
            FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
            Events.OnBodyPartActive += OnBodyPartActive;
            Events.ShowScreen += OnShowScreen;
            if (Data.Instance.userData.IsLogged()) {
                Init();
                Invoke(nameof(InitGalleryDelayed), Time.deltaTime*2);
            }
        }
        private void OnDestroy()
        {
            FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
            Events.OnBodyPartActive -= OnBodyPartActive;
            Events.ShowScreen -= OnShowScreen;
        }

        void OnTokenUpdated() {
            Init();
            Invoke(nameof(InitGalleryDelayed), Time.deltaTime * 2);
        }

        public CharacterPartsHelper.parts part;
        void OnBodyPartActive(CharacterPartsHelper.parts part)
        {
            this.part = part;
        }
        public void ShowBack(bool showIt)
        {
            backBtn.SetActive(showIt);            
        }
        private void OnShowScreen(screenType type)
        {
            undoManager.Reset();
            backToScreen.Add(type);
            switch (type)
            {
                case screenType.StoryMaker:
                    Scenario.Instance.gameObject.SetActive(true);
                    backBtn.SetActive(true);
                    break;
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
            if (uid != "" && uid!=null)
                Data.Instance.cacheData.GetUser(uid, OnUserDone);
            Home();
        }

        private void OnUserDone(CacheData.UserData uData, Texture2D tex)
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
            else if (id == 2) {
                NewCharacter();
                FirebaseAnalytics.LogEvent(
                    "new_character",
                    new Parameter("origin", "home")
                 );
            } else if (id == 3) {
                NewObject(SObjectData.types.generic);
                FirebaseAnalytics.LogEvent(
                    "new_object_generic",
                    new Parameter("origin", "home")
                );
            } else if (id == 4) {
                NewObject(SObjectData.types.background);
                FirebaseAnalytics.LogEvent(
                    "new_object_background",
                    new Parameter("origin", "home")
                );
            }
        }
        public void Albums()
        {
            Events.ShowScreen(UIManager.screenType.Albums);
        }
        public void NewStory() {
            Data.Instance.scenesData.StartNewStory("");
            boardUI.SetEditingType(editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            Invoke(nameof(SetNewStoryEditionState), Time.deltaTime * 2);

            FirebaseAnalytics.LogEvent("new_story");
        }
        void SetNewStoryEditionState() {
            StoryMakerEvents.EnableStoryEdition(true);
        }

        public void NewCharacter()
        {
            Events.OnCharacterReset();
            Events.OnPropReset();
            Events.OnPresetReset();
            Events.EmptySceneItems();
            string newCharacterID = "-On3wQ6Vy9jnpMtTTgWb";
            LoadWork(editingTypes.CHARACTER, newCharacterID);
            Data.Instance.charactersData.SetCurrentID("");
            Events.OnPresetReset();
            Events.ShowScreen(UIManager.screenType.Creation_Character);            
        }
        void InitCharacterScreen()
        {
            Events.ShowScreen(UIManager.screenType.Creation_Character);
        }
        public void NewObject(SObjectData.types type)
        {
            boardUI.items.DeleteAll();
            Data.Instance.sObjectsData.SetType(type);
            boardUI.SetEditingType(editingTypes.OBJECT);
            Events.OnCharacterReset();
            Events.OnPropReset();
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
            if (StoryMakerEvents.isEditing) return;
            switch (type)
            {
                case editingTypes.CHARACTER:
                    Data.Instance.charactersData.SetCurrentID(id);
                    Events.OnCharacterReset();
                    Events.ShowScreen(UIManager.screenType.Creation_Character);
                    break;
                case editingTypes.OBJECT:
                    Events.OnCharacterReset();
                    Events.ShowScreen(UIManager.screenType.Creation_Objects);
                    break;
            }
        }
        public void Back()
        {
             if (backToScreen.Count > 0)
                print("BACK " + backToScreen[backToScreen.Count - 1] );
            else
                print("BACK");
            inputManager.Back();
           if (backToScreen.Count > 0 && backToScreen[backToScreen.Count - 1] == screenType.WorkDetail)
            {
                Events.OnNewBodyPartSelected(null);
                Home();
            }
            else if (CheckLastScreenUnsaved())
            {
                Events.OnConfirm("Vas a perder todos los cambios", "Confirmar y Salir", "Cancelar", ExitConfirmed);
            }
            else if (backToScreen.Count > 0 && backToScreen[backToScreen.Count - 1] == screenType.StoryMaker)
            {
              
                StoryMakerEvents.SetEditing(false);

                if(homePage.screen == HomePage.screens.user)
                    SetBack();
                else
                    Home();
            }
            else
            {
                SetBack();
            }
        }

        bool CheckLastScreenUnsaved() {
            return hasUnsavedChanges &&
                (backToScreen[backToScreen.Count - 1] == screenType.Creation_Character ||
                backToScreen[backToScreen.Count - 1] == screenType.Creation_Objects ||
                backToScreen[backToScreen.Count - 1] == screenType.StoryMaker);
        }

        void SetBack()
        {
            if(StoryMakerEvents.isEditing)
                Events.ShowScreen(screenType.Home);
            else 
             if (backToScreen.Count < 3)
                Events.ShowScreen(screenType.Home);
            else
                Events.ShowScreen(backToScreen[backToScreen.Count - 2]);

            if(backToScreen.Count>1) backToScreen.RemoveAt(backToScreen.Count - 1);
            if (backToScreen.Count > 1) backToScreen.RemoveAt(backToScreen.Count - 1);
        }
        void ExitConfirmed(bool exit)
        {
            if (exit)
                SetBack();
        }
        public void ShowWorkDetail(SOPartData wd)
        {
            if (StoryMakerEvents.isEditing)
            {
                Events.ShowScreen(UIManager.screenType.StoryMaker);
            }
            else
            {
                Events.ShowScreen(UIManager.screenType.WorkDetail);
                workDetailUI.ShowWorkDetail(wd.id, true);
            }
        }
        public void BackToStory(SObjectData newSOData)
        {
            Events.ShowScreen(UIManager.screenType.StoryMaker);  
            StartCoroutine(AddSoAsyncC(newSOData));
        }
        IEnumerator AddSoAsyncC(SObjectData newSOData)
        {
            Events.OnLoading(true);
            yield return new WaitForSeconds(0.5f);
            
            if (newSOData.type == SObjectData.types.generic)
            {
                SODataFabulab data = new SODataFabulab();
                data.id = newSOData.id;
                data.itemName = Utils.GetUniqueDateTimeId();
                StoryMakerEvents.AddSceneObject(data);  
            } else if (newSOData.type == SObjectData.types.background)
            {
                SOBGData data = new SOBGData();
                data.id = newSOData.id;
                data.itemName = Utils.GetUniqueDateTimeId();
                StoryMakerEvents.AddSceneObject(data);  
            } 
            Events.OnLoading(false);                
        }
        public void BackToStoryFromAvatar(string id)
        {            
            Events.ShowScreen(UIManager.screenType.StoryMaker);  
            StartCoroutine(AddAvatarAsyncC(id));
        }
        IEnumerator AddAvatarAsyncC(string id)
        {
            Events.OnLoading(true);
            yield return new WaitForSeconds(0.5f);

            SOAvatarFabulabData data = new SOAvatarFabulabData();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
            StoryMakerEvents.AddSceneObject(data);

            Events.OnLoading(false);           
        }
    }

}