
using Common.UI;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using UI.MainApp.Home.User;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp
{
    public class StoryEditorScreen : MonoBehaviour
    {
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
        [SerializeField] GameObject saveNewStoryButton;
        [SerializeField] GameObject saveStoryButton;
        [SerializeField] GameObject DoneBtn;
        [SerializeField] TMPro.TMP_InputField storyName;
        [SerializeField] bool changesMade;

        bool editionEnabled;

        private void Start()
        {
            CloseTools();
            StoryMakerEvents.ShowSoButtons += ShowSoButtons;
            StoryMakerEvents.OnSaveScene += OnSaveScene;
            StoryMakerEvents.EditActions += EditorActions;
            StoryMakerEvents.EditExpressions += EditExpressions;
            StoryMakerEvents.OnLoadFilm += OnLoadFilm;
            StoryMakerEvents.EnableStoryEdition += EnableStoryEdition;
            StoryMakerEvents.EditAvatar += EditAvatar;
            StoryMakerEvents.EditObject += EditObject;
        }

        void OnDestroy()
        {
            StoryMakerEvents.ShowSoButtons -= ShowSoButtons;
            StoryMakerEvents.OnSaveScene -= OnSaveScene;
            StoryMakerEvents.EditActions -= EditorActions;
            StoryMakerEvents.EditExpressions -= EditExpressions;
            StoryMakerEvents.OnLoadFilm -= OnLoadFilm;
            StoryMakerEvents.EnableStoryEdition -= EnableStoryEdition;
            StoryMakerEvents.EditAvatar -= EditAvatar;
            StoryMakerEvents.EditObject -= EditObject;
        }

        void OnLoadFilm() {
            storyName.text = Data.Instance.scenesData.currentFilmData.name;
        }
        private void OnEnable()
        {
            print("OnEnable EnableStoryEdition " + editionEnabled);
            Invoke(nameof(Init), Time.deltaTime * 4);
        }

        void Init()
        {
            print("Init EnableStoryEdition " + editionEnabled + "frames: " + ScenesManagerFabulab.Instance.Scenes.Count);
            SetChangesMade(false);
            if (!editionEnabled) return;            
            tabs.Init(OnTabClicked, 4);
        }

        public void CloseActionsAndEmojis() {
            actionUI.SetOn(false);
            emojisUI.SetOn(false);
        }
        void ShowSoButtons(Vector3 pos, SOData data)
        {
            if (data is SOAvatarData)
                EditAvatar(data.id);
            print ("ShowSoButtons " + data);
             if (data is SODataFabulab)
                EditObject(data.id);

        }
        string selectedSOId;
        void EditAvatar(string id)
        {
            this.selectedSOId = id;
            editAvatar.SetActive(true);
            actionUI.SetCharacterId(id);
            actionUI.SetOn(true);
            emojisUI.SetOn(false);
        }
        void EditObject(string id)
        {
            this.selectedSOId = id;
            editObjects.SetActive(true);
        }
        public void RemoveSO()
        {
            StoryMakerEvents.RemoveSceneObject();
            CloseTools();
        }
        public void CloseTools()
        {
            editAvatar.SetActive(false);
            editObjects.SetActive(false);
        }
        public void EditorActions() {
            actionUI.SetCharacterId(selectedSOId);
            actionUI.SetOn(true);
            emojisUI.SetOn(false);
        }

        public void EditExpressions() {
            emojisUI.SetCharacterId(selectedSOId);
            actionUI.SetOn(false);
            emojisUI.SetOn(true);
        }

        void OnTabClicked(int id)
        {
            CloseTools();
            actionUI.SetOn(false);
            emojisUI.SetOn(false);
            tabTools.SetOn(id);
            switch (id) {
                case 0:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    backgroundScreen.Init();
                    break;
                case 1:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    characterScreen.Init();
                    break;
                case 2:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    objectsScreen.Init();
                    break;
                case 3:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    textsScreen.Init();
                    break;
                case 4:
                    timeline.SetActive(true);
                    itemList.SetActive(false);
                    break;                    
            }   
        }

        public void OnSaveScene() {
            
            //CreateThumb();
            SceneDataFabulab sdf = ScenesManagerFabulab.Instance.GetActiveScene();
            sdf.Reset();
            SOData bgData = Scenario.Instance.sceneObejctsManager.bgData;
            sdf.bgID = bgData.id;
            foreach (SceneObject so in Scenario.Instance.sceneObejctsManager.sceneObjects) {
                if (so == null || !so.gameObject.activeSelf)
                    continue;

                SOData soData = null;

                if (so is AvatarFabulab) {

                    soData = new SOAvatarFabulabData();
                    soData.Clone(so.GetData());

                    (soData as SOAvatarFabulabData).anim = (so.GetData() as SOAvatarFabulabData).anim;
                    (soData as SOAvatarFabulabData).emoji = (so.GetData() as SOAvatarFabulabData).emoji;
                } else if (so is Prop) {
                    soData = new SODataFabulab();
                    soData.Clone(so.GetData());
                } else if (so is WordBalloon) {
                    soData = new SOWordBalloonData();
                    soData.Clone(so.GetData());
                    (soData as SOWordBalloonData).inputValue = (so.GetData() as SOWordBalloonData).inputValue;
                } else if (so is WordBox) {
                    soData = new SOWordBoxData();
                    soData.Clone(so.GetData());
                    (soData as SOWordBoxData).inputValue = (so.GetData() as SOWordBoxData).inputValue;
                }

                Debug.Log("# Adding to SceneData: " + soData.id + "_"+ soData.itemName + " " + soData);

                sdf.AddSO(soData);
                if ((so is Prop || so is AvatarFabulab) && (Scenario.Instance.sceneObejctsManager as SceneObjectsManagerFabulab).recentAdded.Contains(so)) {
                    ScenesManagerFabulab.Instance.AddItemToNextScenesSameBG(soData);
                    (Scenario.Instance.sceneObejctsManager as SceneObjectsManagerFabulab).recentAdded.Remove(so);
                }

                /*if (customizerData != "" && Data.Instance.scenesData.currentFilmData.IsMyStory())
                    StoryMakerEvents.SetNewAvatarCustomization(customizerData);*/

            }
            if (!changesMade && editionEnabled)
                SetChangesMade(true);
        }

        void SetChangesMade(bool _changesMade) {
            this.changesMade = _changesMade;
            UIManager.Instance.hasUnsavedChanges = this.changesMade;
        }

        public void Save() {
            savePanel.SetActive(false);
            Invoke(nameof(SaveWork), Time.deltaTime*2);
        }
        public void Replace()// Guarda la version editada del personaje.
        {
            Events.SetCharacterIdle("");
            savePanel.SetActive(false);
            SaveWork();
        }
        public void Cancel() {
            savePanel.SetActive(false);
        }

        void SaveWork() {
            UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.charactersData.thumbSize, (tex) => {
                Data.Instance.scenesData.currentFilmData.thumb = tex;
                Data.Instance.scenesData.currentFilmData.name = storyName.text;
                Data.Instance.scenesData.currentFilmData.timestamp = DateTime.UtcNow.ToString("o");
                Data.Instance.scenesData.SaveFilm();
            });            
        }

        void EnableStoryEdition(bool enable) {

            print("EnableStoryEdition " + enable);
            editionEnabled = enable;
            tabs.gameObject.SetActive(enable);
            storyName.interactable = enable;

            actionUI.SetOn(enable);
            emojisUI.SetOn(enable);

            DoneBtn.gameObject.SetActive(enable);

           // Invoke(nameof(SetTabState), Time.deltaTime * 10);
        }

        //void SetTabState() {
        //    int id = editionEnabled ? 0 : 4;
        //    OnTabClicked(id);
        //}
    }
}