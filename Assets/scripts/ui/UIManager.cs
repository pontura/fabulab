using BoardItems;
using BoardItems.BoardData;
using BoardItems.Characters;
using Firebase.Analytics;
using OnBoarding;
using System.Collections;
using System.Collections.Generic;
using UI.MainApp;
using UI.MainApp.Home;
using UI.MainApp.Home.User;
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
        public GamesStories gameStories;

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
            StoryMaker,
            GamesStories,
            GameStoriesCreator
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
            Events.OnAllFilmMetadataLoadDone += Init;
            Events.OnAllUserDataLoadDone += Init;
            Invoke(nameof(InitGalleryDelayed), Time.deltaTime * 2);
            /*if (Data.Instance.userData.IsLogged()) {
                Init();                
            }*/
        }
        private void OnDestroy()
        {
            FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
            Events.OnBodyPartActive -= OnBodyPartActive;
            Events.ShowScreen -= OnShowScreen;
            Events.OnAllFilmMetadataLoadDone -= Init;
            Events.OnAllUserDataLoadDone -= Init;
        }

        void OnTokenUpdated() {
            //Init();
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
                    backToScreen.Clear();
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
        public void Init()
        {
            if (
                Data.Instance.userData.UserDataLoadedDone && 
                Data.Instance.gamesManager.done && 
                Data.Instance.scenesData.ScenesDataLoadedDone) {
                string uid = Data.Instance.userData.userDataInDatabase.uid;
                if (uid != "" && uid != null)
                    Data.Instance.cacheData.GetUser(uid, OnUserDone);
                Debug.Log("# User Likes Count: " + Data.Instance.userData.userDataInDatabase.likes.Count);
                Home();
            } else
                Invoke(nameof(Init), 1);
        }

        private void OnUserDone(CacheData.UserData uData, Texture2D tex)
        {
            print("OnUserDone UID " + uData);
        }

        public void Home()
        {
            Debug.Log("#Home");
            Events.ShowScreen(UIManager.screenType.Home);
        }
        public void Create()
        {
            Events.ShowScreen(UIManager.screenType.Create);
        }
        public void CreateSelected(int id, bool skipOnBoarding = false)
        {
            if (id == 1)
            {
                if(!skipOnBoarding && !onboardingManager.storiesDone)
                    Events.OnBoardingXtraStep(OnBoardingManager.steps.video_story, NewStory);
                else
                    NewStory();
            }
            else if (id == 2) {
                if(!skipOnBoarding && !onboardingManager.charactersDone)
                    Events.OnBoardingXtraStep(OnBoardingManager.steps.video_character, NewCharacter);
                else
                {
                    NewCharacter();
                    FirebaseAnalytics.LogEvent(
                        "new_character",
                        new Parameter("origin", "home")
                    );
                }
            } else if (id == 3) {
                  if(!skipOnBoarding && !onboardingManager.objectsDone)
                    Events.OnBoardingXtraStep(OnBoardingManager.steps.video_object, NewObject);
                else
                {
                    NewObject() ;
                    FirebaseAnalytics.LogEvent(
                        "new_object_generic",
                        new Parameter("origin", "home")
                    );
                }
            } else if (id == 4) {
                 if(!skipOnBoarding && !onboardingManager.bgDone)
                    Events.OnBoardingXtraStep(OnBoardingManager.steps.video_bg, NewBG);
                else
                {
                    NewBG() ;
                    FirebaseAnalytics.LogEvent(
                        "new_object_background",
                        new Parameter("origin", "home")
                    );
                }
            }
        }
        void NewObject()  {  NewObject(SObjectData.types.generic); }
        void NewBG()  {  NewObject(SObjectData.types.background); }

        public void Albums()
        {
            Events.ShowScreen(UIManager.screenType.Albums);
        }
        void NewStory() {
            Data.Instance.scenesData.StartNewStory("");
            boardUI.SetEditingType(editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            Invoke(nameof(SetNewStoryEditionState), Time.deltaTime * 2);

            FirebaseAnalytics.LogEvent("new_story");
        }
        void SetNewStoryEditionState() {
            StoryMakerEvents.EnableStoryEdition(true);
        }

        void NewCharacter()
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
        void NewObject(SObjectData.types type)
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
        public void AddBackTo(screenType type, bool resetAll = false)
        {
            if (resetAll)
                backToScreen.Clear();
            print("AddBackTo " + type + " resetAll: " + resetAll   );
            backToScreen.Add(type);
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
            else if (backToScreen.Count > 0 && backToScreen[backToScreen.Count - 1] == screenType.GameStoriesCreator)
            {
                gameStories.BackToPlay();
                AddBackTo(UIManager.screenType.GamesStories, true);
            }
            else if (backToScreen.Count > 0 && backToScreen[backToScreen.Count - 1] == screenType.GamesStories)
            {
                ReOpenGames();
            }
            else if (backToScreen.Count > 0 && backToScreen[backToScreen.Count - 1] == screenType.StoryMaker)
            {
                if (backToScreen.Count > 1 && backToScreen[backToScreen.Count - 2] == screenType.GamesStories)
                {
                    gameStories.ShowFromHome(true);
                    AddBackTo(UIManager.screenType.GamesStories, true);
                    return;
                }
                // if( Data.Instance.gamesManager.watchingFilmsMade)
                // {
                //     gameStories.ShowFromHome(true);
                //     return;
                // }
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
            if(backToScreen.Count >0 && backToScreen[backToScreen.Count - 1] == screenType.GameStoriesCreator)
                return true;
            return hasUnsavedChanges &&
                (backToScreen[backToScreen.Count - 1] == screenType.Creation_Character ||
                backToScreen[backToScreen.Count - 1] == screenType.Creation_Objects ||
                backToScreen[backToScreen.Count - 1] == screenType.StoryMaker);
        }
        void ReOpenGames()
        {
            hasUnsavedChanges = false;        
            Data.Instance.gamesManager.SetPlaying(false);
            gameStories.BackToPlay();
            AddBackTo(UIManager.screenType.StoryMaker, true);
        }
        void SetBack()
        {
            if(StoryMakerEvents.isEditing)
                Events.ShowScreen(screenType.Home);
            else if (backToScreen.Count < 3)
            {
                if(backToScreen[backToScreen.Count - 1] == screenType.GameStoriesCreator)
                    ReOpenGames();
                else
                    Events.ShowScreen(screenType.Home);
            }
            else if(Data.Instance.gamesManager.IsEditing())
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