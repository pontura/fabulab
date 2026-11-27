using BoardItems;
using BoardItems.BoardData;
using BoardItems.Characters;
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
            GameStoriesCreator,
            MiniGame,
            MiniGameEditor
        }

        public static UIManager Instance {
            get {
                return mInstance;
            }
        }
        [SerializeField] List<screenType> backToScreen;
        void Awake() {
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
        private void Start() {
            FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
            FirebaseAuthManager.Instance.OnSignedOut += OnSignedOut;
            Events.OnBodyPartActive += OnBodyPartActive;
            Events.ShowScreen += OnShowScreen;
            Events.OnAllFilmMetadataLoadDone += Init;
            Events.OnAllUserDataLoadDone += Init;
            Invoke(nameof(InitGalleryDelayed), Time.deltaTime * 2);
            /*if (Data.Instance.userData.IsLogged()) {
                Init();                
            }*/
        }
        private void OnDestroy() {
            FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
            FirebaseAuthManager.Instance.OnSignedOut -= OnSignedOut;
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
        void OnBodyPartActive(CharacterPartsHelper.parts part) {
            this.part = part;
        }
        public void ShowBack(bool showIt) {
            backBtn.SetActive(showIt);
        }
        private void OnShowScreen(screenType type) {
            Debug.Log("? OnShowScreen");
            undoManager.Reset();
			AddBackTo(type);
            switch (type) {
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
        // The home is shown once the data of the signed in user is loaded. Init is called by several events
        // (and retries itself): without this, each call showed the home again, taking the user out of an editor
        bool homeShownAfterLoad;

        void OnSignedOut() {
            homeShownAfterLoad = false;
        }

        public void Init() {
            print("uimanager init");
            // One pending retry at most, whoever called
            CancelInvoke(nameof(Init));

            if (
                Data.Instance.userData.UserDataLoadedDone &&
                Data.Instance.gamesManager.done &&
                Data.Instance.scenesData.ScenesDataLoadedDone) {
                if (homeShownAfterLoad)
                    return;
                homeShownAfterLoad = true;
                string uid = Data.Instance.userData.userDataInDatabase.uid;
                if (uid != "" && uid != null)
                    Data.Instance.cacheData.GetUser(uid, OnUserDone);
                Debug.Log("# User Likes Count: " + Data.Instance.userData.userDataInDatabase.likes.Count);
                Home();
            } else
                Invoke(nameof(Init), 1);
        }

        private void OnUserDone(CacheData.UserData uData, Texture2D tex) {
            print("OnUserDone UID " + uData);
        }
        public void HomeCharacters() {
            Debug.Log("#Home Characters");
            homePage.OnSelected(HomePage.screens.characters);
            Events.ShowScreen(UIManager.screenType.Home);
        }
        public void Home() {
            Debug.Log("#Home");
            Events.ShowScreen(UIManager.screenType.Home);
        }
        public void Create() {
            Events.ShowScreen(UIManager.screenType.Create);
        }
        public void MiniGames() {
            Events.ShowScreen(UIManager.screenType.MiniGame);
        }
        public void CreateSelected(int id, bool skipOnBoarding = false) {
            editedFromStory = null;
            if (id == 1) {
                if (!skipOnBoarding && !Data.Instance.userData.userDataInDatabase.onboardings.stories)
                    Events.OnBoardingXtraStep(OnBoardingManager.steps.video_story, NewStory);
                else
                    NewStory();
            } else if (id == 2) {
                if (!skipOnBoarding && !Data.Instance.userData.userDataInDatabase.onboardings.characters)
                    Events.OnBoardingXtraStep(OnBoardingManager.steps.video_character, NewCharacter);
                else {
                    NewCharacter();
                    FabulabAnalytics.LogEvent(
                        "new_character",
                        new System.Collections.Generic.Dictionary<string, object> { { "origin", "home" } }
                    );
                }
            } else if (id == 3) {
                if (!skipOnBoarding && !Data.Instance.userData.userDataInDatabase.onboardings.objects)
                    Events.OnBoardingXtraStep(OnBoardingManager.steps.video_object, NewObject);
                else {
                    NewObject();
                    FabulabAnalytics.LogEvent(
                        "new_object_generic",
                        new System.Collections.Generic.Dictionary<string, object> { { "origin", "home" } }
                    );
                }
            } else if (id == 4) {
                if (!skipOnBoarding && !Data.Instance.userData.userDataInDatabase.onboardings.bgs)
                    Events.OnBoardingXtraStep(OnBoardingManager.steps.video_bg, NewBG);
                else {
                    NewBG();
                    FabulabAnalytics.LogEvent(
                        "new_object_background",
                        new System.Collections.Generic.Dictionary<string, object> { { "origin", "home" } }
                    );
                }
            }
        }
        void NewObject() { NewObject(SObjectData.types.generic); }
        void NewBG() { NewObject(SObjectData.types.background); }

        public void Albums() {
            Events.ShowScreen(UIManager.screenType.Albums);
        }
        void NewStory() {
            Data.Instance.scenesData.StartNewStory("");
            boardUI.SetEditingType(editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            Invoke(nameof(SetNewStoryEditionState), Time.deltaTime * 2);

            FabulabAnalytics.LogEvent("new_story");
        }
        void SetNewStoryEditionState() {
            StoryMakerEvents.EnableStoryEdition(true);
        }

        void NewCharacter() {
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
        void InitCharacterScreen() {
            Events.ShowScreen(UIManager.screenType.Creation_Character);
        }
        void NewObject(SObjectData.types type) {
            boardUI.items.DeleteAll();
            Data.Instance.sObjectsData.CopySourceId = ""; // made from scratch
            Data.Instance.sObjectsData.SetType(type);
            boardUI.SetEditingType(editingTypes.OBJECT);
            Events.OnCharacterReset();
            Events.OnPropReset();
            Events.OnPresetReset();
            GaleriasData.GalleryData gd = Data.Instance.galeriasData.GetGallery(1);
            Events.InitGallery(gd, true, InitObjectsScreen);
            Events.EmptySceneItems();
        }
        void InitObjectsScreen() {
            Events.ShowScreen(UIManager.screenType.Creation_Objects);
        }
        public void LoadWork(editingTypes type, string id) {
            boardUI.SetEditingType(type);
            boardUI.LoadWork(id);
            if (StoryMakerEvents.isEditing) return;
            ShowBoardEditor(type, id);
        }

        void ShowBoardEditor(editingTypes type, string id) {
            switch (type) {
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
        public void AddBackTo(screenType type, bool resetAll = false) {
            if (resetAll)
                backToScreen.Clear();

            if (backToScreen.Count>0 && backToScreen[backToScreen.Count - 1] == type)
                return;

			print("AddBackTo " + type + " resetAll: " + resetAll);            
            backToScreen.Add(type);
        }
        public void Back() {
            if (backToScreen.Count > 0)
                print("BACK " + backToScreen[backToScreen.Count - 1]);
            else
                print("BACK");
            inputManager.Back();
            
            StoryEditorScreen storyEditorScreen = GetComponentInChildren<StoryEditorScreen>();
            if ( storyEditorScreen != null && storyEditorScreen.saveMenu.isOn)
            {
                storyEditorScreen.CloseSaveMenu();
                return;
            }
            if (backToScreen.Count > 0
            && StoryMakerEvents.isEditing
            && (backToScreen[backToScreen.Count - 1] == screenType.Creation_Character ||
            backToScreen[backToScreen.Count - 1] == screenType.Creation_Objects)
            ) {
				if (backToScreen.Count > 1) backToScreen.RemoveAt(backToScreen.Count - 1);
				editedFromStory = null;
				Events.ShowScreen(UIManager.screenType.StoryMaker);
			} else if (backToScreen.Count > 0 && backToScreen[backToScreen.Count - 1] == screenType.WorkDetail) {
                Events.OnNewBodyPartSelected(null);
                Home();
            } else if (CheckLastScreenUnsaved()) {
                Events.OnConfirm("Vas a perder todos los cambios", "Confirmar y Salir", "Cancelar", ExitConfirmed);
            } else if (backToScreen.Count > 0 && backToScreen[backToScreen.Count - 1] == screenType.GameStoriesCreator) {
                gameStories.BackToPlay();
                AddBackTo(UIManager.screenType.GamesStories, true);
            } else if (backToScreen.Count > 0 && backToScreen[backToScreen.Count - 1] == screenType.GamesStories) {
                ReOpenGames();
            } else if (backToScreen.Count > 0 && backToScreen[backToScreen.Count - 1] == screenType.StoryMaker) {
                Data.Instance.gamesManager.Watching = false;
                if (backToScreen.Count > 1 && backToScreen[backToScreen.Count - 2] == screenType.GamesStories) {
                    
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

                if (homePage.screen == HomePage.screens.user)
                    SetBack();
                else
                    Home();
            } else {
                SetBack();
            }
        }

        bool CheckLastScreenUnsaved() {
            if (backToScreen.Count > 0 && backToScreen[backToScreen.Count - 1] == screenType.GameStoriesCreator)
                return true;
            return hasUnsavedChanges &&
                (backToScreen[backToScreen.Count - 1] == screenType.Creation_Character ||
                backToScreen[backToScreen.Count - 1] == screenType.Creation_Objects ||
                backToScreen[backToScreen.Count - 1] == screenType.StoryMaker ||
                backToScreen[backToScreen.Count - 1] == screenType.MiniGameEditor);
        }
        void ReOpenGames() {
            hasUnsavedChanges = false;
            Data.Instance.gamesManager.SetPlaying(false);
            gameStories.BackToPlay();
            AddBackTo(UIManager.screenType.StoryMaker, true);
        }
        void SetBack() {
            if (StoryMakerEvents.isEditing)
                Events.ShowScreen(screenType.Home);
            else if (backToScreen.Count < 3) {
                if (backToScreen[backToScreen.Count - 1] == screenType.GameStoriesCreator)
                    ReOpenGames();
                else
                    Events.ShowScreen(screenType.Home);
            } else if (Data.Instance.gamesManager.IsEditing())
                Events.ShowScreen(screenType.Home);
            else
                Events.ShowScreen(backToScreen[backToScreen.Count - 2]);

            if (backToScreen.Count > 1) backToScreen.RemoveAt(backToScreen.Count - 1);
            if (backToScreen.Count > 1) backToScreen.RemoveAt(backToScreen.Count - 1);
        }
        void ExitConfirmed(bool exit) {
            if (exit)
                SetBack();
        }
        public void ShowWorkDetail(SOPartData wd) {
            if (StoryMakerEvents.isEditing) {
                Events.ShowScreen(UIManager.screenType.StoryMaker);
            } else {
                Events.ShowScreen(UIManager.screenType.WorkDetail);
                workDetailUI.ShowWorkDetail(wd.id, true);
            }
        }
        // Opens a story in the story editor to edit it (like InfoDataScreen.OpenWork). One of another user is saved
        // as a new story of the user (StoryEditorScreen.InitSaving). False when the story is not loaded
        public bool OpenStoryForEdition(string id) {
            ScenesData scenes = Data.Instance.scenesData;
            bool own = scenes.userFilmsData.Exists(f => f.id == id);
            if (!own && !scenes.filmsData.Exists(f => f.id == id))
                return false;
            Events.OnLoadingParent(null, () => {
                Events.OnLoading(true);
                if (own)
                    scenes.LoadUserFilm(id);
                else
                    scenes.LoadFilm(id);
                boardUI.SetEditingType(editingTypes.NONE);
                Events.ShowScreen(UIManager.screenType.StoryMaker);
                Invoke(nameof(SetNewStoryEditionState), Time.deltaTime * 2);
            });
            return true;
        }

        // ---------- Editing a work of the story ----------

        // A character, object or background of the story, opened in the board editor from the story editor
        class EditedWork {
            public editingTypes type;
            public SObjectData.types objectType;
            public string id;
        }
        EditedWork editedFromStory;

        // True while a work of the story is edited in the board (see EditWorkFromStory)
        public bool IsEditingWorkFromStory => editedFromStory != null;

        // A character of another user opened to modify it (WebEditorController): the character editor opens in
        // edition mode instead of preview. Read (and cleared) by CharacterEdition when it is shown
        public bool OpenOthersInEdition { get; set; }

        // Opens a character, object or background of the story in the board editor. An own work is edited as it
        // is; a work of another user is saved as a new work of the user (CharacterEdition.Save / ObjectsEdition.Done
        // save as new what is not in the works of the user). Once saved, the story shows the saved one in place of
        // the edited one, in all its keyframes (BackToStoryFromEdition)
        public void EditWorkFromStory(editingTypes type, string id, SObjectData.types objectType = SObjectData.types.generic) {
            if (string.IsNullOrEmpty(id))
                return;
            editedFromStory = new EditedWork { type = type, objectType = objectType, id = id };
            // Like AddNew: while it is on, saving or leaving the board goes back to the story (CharacterEdition.OnSaved,
            // ObjectsEdition.OnSoSaved, Back) instead of the home, that would close the web editor.
            // StoryEditorScreen.Init sets it again from the story once back there
            StoryMakerEvents.SetEditing(true);
            if (type == editingTypes.OBJECT)
                Data.Instance.sObjectsData.SetType(objectType);
            boardUI.SetEditingType(type);
            boardUI.LoadWork(id);
            ShowBoardEditor(type, id);
        }

        // The edited work was saved with this id: the same one (own work) or a new one (copy of another user's)
        void BackToStoryFromEdition(string savedId) {
            EditedWork edited = editedFromStory;
            editedFromStory = null;
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            StartCoroutine(ShowEditedWorkC(edited, savedId));
        }
        IEnumerator ShowEditedWorkC(EditedWork edited, string savedId) {
            Events.OnLoading(true);
            yield return new WaitForSeconds(0.5f);

            SceneObjectsManager sceneObjects = Scenario.Instance.sceneObjectsManager;
            if (edited.type == editingTypes.CHARACTER)
                sceneObjects.ReplaceWork(edited.id, savedId, true);
            else if (edited.objectType == SObjectData.types.background)
                sceneObjects.ReplaceBackground(edited.id, savedId);
            else
                sceneObjects.ReplaceWork(edited.id, savedId, false);

            Events.OnLoading(false);
        }

        public void BackToStory(SObjectData newSOData) {
            if (editedFromStory != null) {
                BackToStoryFromEdition(newSOData.id);
                return;
            }
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            StartCoroutine(AddSoAsyncC(newSOData));
        }
        IEnumerator AddSoAsyncC(SObjectData newSOData) {
            Events.OnLoading(true);
            yield return new WaitForSeconds(0.5f);

            if (newSOData.type == SObjectData.types.generic) {
                SOPropData data = new SOPropData();
                data.id = newSOData.id;
                data.itemName = Utils.GetUniqueDateTimeId();
                Scenario.Instance.sceneObjectsManager.AddNewSceneObject(data);
            } else if (newSOData.type == SObjectData.types.background) {
                SOBGData data = new SOBGData();
                data.id = newSOData.id;
                data.itemName = Utils.GetUniqueDateTimeId();
                Scenario.Instance.sceneObjectsManager.AddNewSceneObject(data);
            }
            Events.OnLoading(false);
        }
        public void BackToStoryFromAvatar(string id) {
            if (editedFromStory != null) {
                BackToStoryFromEdition(id);
                return;
            }
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            StartCoroutine(AddAvatarAsyncC(id));
        }
        IEnumerator AddAvatarAsyncC(string id) {
            Events.OnLoading(true);
            yield return new WaitForSeconds(0.5f);

            SOAvatarData data = new SOAvatarData();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
            Scenario.Instance.sceneObjectsManager.AddNewSceneObject(data);

            Events.OnLoading(false);
        }
    }

}