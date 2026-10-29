using BoardItems;
using Common.UI;
using Firebase.Analytics;
using OnBoarding;
using System;
using System.Collections.Generic;
using UI.MainApp.Home.User;
using UnityEngine;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;
using Avatar = Yaguar.StoryMaker.Editor.Avatar;

namespace UI.MainApp
{
    public class StoryEditorScreen : MonoBehaviour
    {
        AvatarSelectionScreen avatarSelectionScreen;
        ObjectSelectionScreen objectSelectionScreen;
        [SerializeField] ToolsMenuStories toolsStories;
        [SerializeField] GameObject arrowSelect;
        [SerializeField] TabToolsManager tabTools;
        [SerializeField] TabController tabs;
        [SerializeField] GameObject timeline;
        [SerializeField] GameObject itemList;
        [SerializeField] Transform itemListContainer;
        [SerializeField] AvatarSelectionScreen characterScreen;
        [SerializeField] ObjectSelectionScreen objectsScreen;
        [SerializeField] BackgroundSelectionScreen backgroundScreen;
        [SerializeField] TextSelectionScreen textsScreen;
        [SerializeField] ActionsUI actionUI;
        [SerializeField] EmojisUI emojisUI;
        [SerializeField] GameObject editAvatar;
        [SerializeField] GameObject editObjects;

        [SerializeField] GameObject savePanel;
        [SerializeField] GameObject deleteStoryButton;
        public SaveMenu saveMenu;
        [SerializeField] GameObject saveBtn;
        [SerializeField] TMPro.TMP_InputField storyName;
        [SerializeField] bool changesMade;
        AddNew addNewPanel;

        //bool editionEnabled;
              

        private void Start()
        {
            saveMenu.Show(false);
            avatarSelectionScreen = GetComponent<AvatarSelectionScreen>();
            objectSelectionScreen = GetComponent<ObjectSelectionScreen>();
            addNewPanel = GetComponent<AddNew>();
            CloseTools();
            StoryMakerEvents.ShowSaveBtn += ShowSaveBtn;
            StoryMakerEvents.ShowSoButtons += ShowSoButtons;
            StoryMakerEvents.OnStoryChanged += OnStoryChanged;
            StoryMakerEvents.EditActions += EditorActions;
            StoryMakerEvents.EditExpressions += EditExpressions;
            StoryMakerEvents.OnLoadFilm += OnLoadFilm;
            StoryMakerEvents.EnableStoryEdition += EnableStoryEdition;
            StoryMakerEvents.NoneItemSelected += NoneItemSelected;
            StoryMakerEvents.OnStartNewStory += OnStartNewStory;
        }
         void OnDisable()
         {
            actionUI.SetOn(false);
            emojisUI.SetOn(false);
            timeline.SetActive(true);
            itemList.SetActive(false);
            characterScreen.Show(false);
            backgroundScreen.Show(false);
            objectsScreen.Show(false);
         }
        void OnDestroy()
        {
            StoryMakerEvents.ShowSaveBtn -= ShowSaveBtn;
            StoryMakerEvents.ShowSoButtons -= ShowSoButtons;
            StoryMakerEvents.OnStoryChanged -= OnStoryChanged;
            StoryMakerEvents.EditActions -= EditorActions;
            StoryMakerEvents.EditExpressions -= EditExpressions;
            StoryMakerEvents.OnLoadFilm -= OnLoadFilm;
            StoryMakerEvents.EnableStoryEdition -= EnableStoryEdition;
            StoryMakerEvents.NoneItemSelected -= NoneItemSelected;
            StoryMakerEvents.OnStartNewStory -= OnStartNewStory;
        }

        private void ShowSaveBtn(bool isOn)
        {
            print("ShowSaveBtn " + isOn);
            saveBtn.gameObject.SetActive(isOn);
        }

        public void ShowToolsInStories(bool isOn)
        {
            print("Show tools in stories " + isOn);
            toolsStories.Show(isOn);
        }
        
        public void OpenTools() {
            ShowToolsInStories(true);
        }
        void OnStartNewStory() {
            storyName.text = Data.Instance.scenesData.currentFilmData.name;
        }

        void OnLoadFilm() {
            storyName.text = Data.Instance.scenesData.currentFilmData.name;
        }
        private void OnEnable()
        {
            Invoke(nameof(Init), Time.deltaTime * 4);
        }

        public void Init()
        {
            print("Init EnableStoryEdition " + GetComponent<FilmMakerManager>().isEditing + " frames: " + ScenesManager.Instance.Scenes.Count);
            
            StoryMakerEvents.ShowSaveBtn(GetComponent<FilmMakerManager>().isEditing);
            if (!StoryMakerEvents.isEditing)
                SetChangesMade(false);
            else
                StoryMakerEvents.SetEditing(GetComponent<FilmMakerManager>().isEditing);  
                
            if (GetComponent<FilmMakerManager>().isEditing)
                tabs.Init(OnTabClicked, GetInitialTab());
            else
                ShowPlayerView();
        }

        // Watching a story: only the timeline, without the edition lists
        void ShowPlayerView() {
            timeline.SetActive(true);
            itemList.SetActive(false);
            characterScreen.Show(false);
            backgroundScreen.Show(false);
            objectsScreen.Show(false);
        }

        // A new story starts in the backgrounds tab until it has a background; otherwise, in the film tab
        int GetInitialTab() {
            SceneData scene = ScenesManager.Instance.GetActiveScene();
            bool withoutBackground = scene == null || string.IsNullOrEmpty(scene.bgID);
            return Data.Instance.scenesData.IsNewStory && withoutBackground ? 0 : 4;
        }

        public void SelectTab(int id) {
            tabs.Init(OnTabClicked, id);
        }

        public void CloseActionsAndEmojis() {
            actionUI.SetOn(false);
            emojisUI.SetOn(false);
        }
        void ShowSoButtons(Vector3 pos, SOData data)
        {
            if (data is SOAvatarData)
                EditAvatar(pos, data.id);
            print ("ShowSoButtons " + data);
            if (data is SOPropData)
                EditObject(pos, data.id);
            else if (data is SOWordBalloonData)
            {
                ShowToolsInStories(false);
                arrowSelect.SetActive(true);
                arrowSelect.transform.position = pos;
            }

        }
        string selectedSOId;
        void EditAvatar(Vector3 pos, string id)
        {
            NoneItemSelected();
            arrowSelect.SetActive(true);
            arrowSelect.transform.position = pos;
            this.selectedSOId = id;
            editAvatar.SetActive(true);
            actionUI.SetCharacterId(id);
            actionUI.SetOn(true);
            emojisUI.SetOn(false);
        }
        void EditObject(Vector3 pos, string id)
        {
            ShowToolsInStories(true);
            arrowSelect.SetActive(true);
            arrowSelect.transform.position = pos;
            this.selectedSOId = id;
            editObjects.SetActive(true);
        }
        public void RemoveSO()
        {
            Scenario.Instance.sceneObjectsManager.RemoveSelected();
            CloseTools();
        }

        void NoneItemSelected() {
            characterScreen.ReplaceEnabled = false;
            objectsScreen.ReplaceEnabled = false;
        }

        public void CancelSelection() {
            NoneItemSelected();
            CloseTools();
            Scenario.Instance.sceneObjectsManager.selected = null;
        }

        public void CloseTools()
        {
            ShowToolsInStories(false);
            arrowSelect.SetActive(false);
            editAvatar.SetActive(false);
            editObjects.SetActive(false);
            if (Scenario.Instance.sceneObjectsManager.selected != null) {
                if (Scenario.Instance.sceneObjectsManager.selected is Avatar avatar) {
                    if(!characterScreen.ReplaceEnabled)
                        avatar?.Borders?.Show(false);
                } else if (Scenario.Instance.sceneObjectsManager.selected is Prop prop) {
                    if (!objectsScreen.ReplaceEnabled)
                        prop?.Borders?.Show(false);
                }
            }            
            GetComponent<EditFieldUI>().ClosePanel();
        }
        public void EditorActions() {            
            ShowToolsInStories(false);
            actionUI.SetCharacterId(selectedSOId);
            actionUI.SetOn(true);
            emojisUI.SetOn(false);
        }

        public void EditExpressions() {            
             ShowToolsInStories(false);
            emojisUI.SetCharacterId(selectedSOId);
            actionUI.SetOn(false);
            emojisUI.SetOn(true);
        }

        public void SelectedFlipX() {
            if (Scenario.Instance.sceneObjectsManager.selected != null) {
                Scenario.Instance.sceneObjectsManager.selected.FlipX();
                Scenario.Instance.sceneObjectsManager.Commit(Scenario.Instance.sceneObjectsManager.selected);
            }
        }

        public void ReplaceCharacter() {            
             ShowToolsInStories(false);
            if (Scenario.Instance.sceneObjectsManager.selected != null) {
                characterScreen.ReplaceEnabled = true;
                objectSelectionScreen.Cancel();
                avatarSelectionScreen.Cancel();
                CloseTools();
                actionUI.SetOn(false);
                emojisUI.SetOn(false);
                tabTools.SetOn(1);
                timeline.SetActive(false);
                itemList.SetActive(true);
                characterScreen.Show(true);
                backgroundScreen.Show(false);
                objectsScreen.Show(false);            
                if (Scenario.Instance.sceneObjectsManager.selected is Avatar avatar) {
                    avatar?.Borders?.Show(true);
                }
                tabs.SetActive(1);
            }
        }
        public void ReplaceProp() {
            if (Scenario.Instance.sceneObjectsManager.selected != null) {
                objectsScreen.ReplaceEnabled = true;
                objectSelectionScreen.Cancel();
                avatarSelectionScreen.Cancel();
                CloseTools();
                actionUI.SetOn(false);
                emojisUI.SetOn(false);
                tabTools.SetOn(2);
                timeline.SetActive(false);
                itemList.SetActive(true);
                characterScreen.Show(false);
                backgroundScreen.Show(false);
                objectsScreen.Show(true);
                if (Scenario.Instance.sceneObjectsManager.selected is Prop prop) {
                    prop?.Borders?.Show(true);
                }
                tabs.SetActive(2);
            }
        }

        public void OnTabClicked(int id){
             ShowToolsInStories(false);
            NoneItemSelected();
            objectSelectionScreen.Cancel();
            avatarSelectionScreen.Cancel();
            addNewPanel.Close();
            CloseTools();
            actionUI.SetOn(false);
            emojisUI.SetOn(false);
            tabTools.SetOn(id);
            switch (id) {
                case 0:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    backgroundScreen.Show(true);
                    objectsScreen.Show(false);
                    characterScreen.Show(false);
                    Events.OnPopupTopSignalText("Fondos");
                    break;
                case 1:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    characterScreen.Show(true);
                    backgroundScreen.Show(false);
                    objectsScreen.Show(false);
                    Events.OnPopupTopSignalText("Personajes");
                    break;
                case 2:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    characterScreen.Show(false);
                    backgroundScreen.Show(false);
                    objectsScreen.Show(true);
                    Events.OnPopupTopSignalText("Objetos");
                    break;
                case 3:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    characterScreen.Show(false);
                    backgroundScreen.Show(false);
                    objectsScreen.Show(false);
                    textsScreen.Show(true);
                    Events.OnPopupTopSignalText("Di�logos");
                    break;
                case 4:
                    timeline.SetActive(true);
                    itemList.SetActive(false);
                    characterScreen.Show(false);
                    backgroundScreen.Show(false);
                    objectsScreen.Show(false);
                    Events.OnPopupTopSignalText("Film   ");
                    break;                    
            }   
        }

        void OnStoryChanged() {
            if (!changesMade && GetComponent<FilmMakerManager>().isEditing)
                SetChangesMade(true);
        }

        void SetChangesMade(bool _changesMade) {
            this.changesMade = _changesMade;
            UIManager.Instance.hasUnsavedChanges = this.changesMade;
        }

        public void SaveDialog() {
            savePanel.SetActive(true);
        }
        public void CloseSaveMenu()
        {
            saveMenu.Show(false);
        }
        public void StoryReadyClicked()
        {            
            saveMenu.Show(true);
            if(Data.Instance.gamesManager.playing)
                saveMenu.SetGame();
            else
            {
                FilmData fdata = Data.Instance.scenesData.GetMeta(ScenesManager.Instance.currentFDataID);
                if(fdata != null && fdata.isPublic)
                    saveMenu.SetPublic(true);
                else
                    saveMenu.SetPublic(false);
            }
        }
        bool saveAndPublish;
        public void SaveAndPublish()
        {
            saveAndPublish = true;
            InitSaving();
        }
        public void SaveDraft()
        {   
            saveAndPublish = false;
            InitSaving();
        }
         void InitSaving()
        {
            CloseSaveMenu();
            Events.OnLoading(true);
            if (ScenesManager.Instance.currentFDataID == "")
                Save();
            else
            {
                savePanel.SetActive(false);
                Invoke(nameof(SaveWork), Time.deltaTime * 2);
            }
            FabulabAnalytics.LogEvent("story_save_clicked");
        }
        public void Cancel() {
            savePanel.SetActive(false);
        }
        void Save() {
            Data.Instance.scenesData.currentFilmData = new FilmData();
            Data.Instance.scenesData.currentFilmData.name = storyName.text;
            ScenesManager.Instance.currentFilmData = Data.Instance.scenesData.currentFilmData;
            Data.Instance.scenesData.currentFilmData.userID = Data.Instance.userData.userDataInDatabase.uid;
            ScenesManager.Instance.currentFDataID = "";
            savePanel.SetActive(false);
            Invoke(nameof(SaveWork), Time.deltaTime * 2);
        }
        void SaveWork() {
            UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.scenesData.ThumbSize, (tex) => {
                Data.Instance.scenesData.currentFilmData.thumb = tex;
                Data.Instance.scenesData.currentFilmData.name = storyName.text;
                Data.Instance.scenesData.currentFilmData.timestamp = DateTime.UtcNow.ToString("o");
                Data.Instance.scenesData.currentFilmData.tags = new System.Collections.Generic.List<string>();
                
                Data.Instance.scenesData.SaveFilm(OnSavedDone);
                SetChangesMade(false);
            });
        }
        void OnSavedDone(bool succes, string id) {
             
            if (succes)
            { 
                print("Historia guardada con id: " + id + " saveAndPublish: " + saveAndPublish);
                if(saveAndPublish)
                {
                    print("Publicando: " + id);
                    Data.Instance.scenesData.SaveInfo(id, true, new List<string>(), OnPublishDone);
                    UIManager.Instance.Back();
                } else
                {      
                    Events.OnLoading(false);           
                    Events.OnConfirm("Querés continuar la historia o salir?", "Continuar", "Salir", OnConfirmExit);
                }
                Events.OnPopupTopSignalText("Historia guardada");
            }
            else
            {
                Events.OnLoading(false); 
                Events.OnPopupTopSignalText("Error al guardar la historia");
            }
        }

        private void OnPublishDone(bool arg1, string arg2)
        {
            Events.OnLoading(false);  
        }

        void OnConfirmExit(bool ok) {
            if (ok)
            {
                print("Continuar historia");
            } else
            {
                print("Salir historia");
                UIManager.Instance.Back();
            }
        }
        public void Delete() {
            Events.OnConfirm("Confirm�s que quer�s borrar esta historia?", "SI", "NO", OnConfirm);
        }
        void OnConfirm(bool ok) {
            if (ok)
            {
                Events.OnLoading(true);
                FirebaseStoryMakerDBManager.Instance.DeleteFilm(Data.Instance.scenesData.currentFilmData, OnDeleted);
            }
        }
        public void OnDeleted(string filmId) {
            Data.Instance.scenesData.currentFilmData = null;
            ScenesManager.Instance.currentFilmData = null;
            ScenesManager.Instance.currentFDataID = "";
            Data.Instance.scenesData.RemoveFD(filmId);
            savePanel.SetActive(false);
            UIManager.Instance.Back();
        }

        void EnableStoryEdition(bool enable) {
            print("EnableStoryEdition " + enable);
            GetComponent<FilmMakerManager>().isEditing = enable;
            tabs.gameObject.SetActive(enable);
            storyName.gameObject.SetActive(enable);
            actionUI.SetOn(enable);
            emojisUI.SetOn(enable);
            if (!enable)
                ShowPlayerView();
        }
        
        [SerializeField] OnBoarding_Movie onboarding_Movie;
        public void HelpClicked()
        {
            onboarding_Movie.Help();
        }

        [SerializeField] float scaleFactor = 30;
        public void Scale(bool up) {
            if (Scenario.Instance.sceneObjectsManager.selected != null) {
                float r = Scenario.Instance.sceneObjectsManager.selected.transform.localScale.x;
                if (up) r += scaleFactor; else r -= scaleFactor;
                Scenario.Instance.sceneObjectsManager.selected.Resize(r);
                Scenario.Instance.sceneObjectsManager.Commit(Scenario.Instance.sceneObjectsManager.selected);
            }
        }
        public void Rotate(bool left) {
            if (Scenario.Instance.sceneObjectsManager.selected != null) {
                float value = 7.5f;
                float r = Scenario.Instance.sceneObjectsManager.selected.GetData().rot;
                if (left) r += value; else r -= value;
                Scenario.Instance.sceneObjectsManager.selected.Rotate(r);
                Scenario.Instance.sceneObjectsManager.Commit(Scenario.Instance.sceneObjectsManager.selected);
            }
        }
    }
}
