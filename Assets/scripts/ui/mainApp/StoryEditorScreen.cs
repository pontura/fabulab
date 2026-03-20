using BoardItems.Characters;
using Common.UI;
using System.Security.Cryptography.X509Certificates;
using UI.MainApp.Home.User;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp
{
    public class StoryEditorScreen : MonoBehaviour
    {
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

        [SerializeField] GameObject savePanel;
        [SerializeField] GameObject deleteStoryButton;
        [SerializeField] GameObject saveNewStoryButton;
        [SerializeField] GameObject saveStoryButton;
        [SerializeField] GameObject DoneBtn;
        [SerializeField] TMPro.TMP_InputField storyName;
        [SerializeField] bool changesMade;

        bool editionEnabled;

        private void Start() {
            StoryMakerEvents.OnSaveScene += OnSaveScene;
            StoryMakerEvents.EditActions += EditorActions;
            StoryMakerEvents.EditExpressions += EditExpressions;
            StoryMakerEvents.OnLoadFilm += OnLoadFilm;
            StoryMakerEvents.EnableStoryEdition += EnableStoryEdition;
            Invoke(nameof(Init), Time.deltaTime * 4);
        }

        void OnDestroy() {
            StoryMakerEvents.OnSaveScene -= OnSaveScene;
            StoryMakerEvents.EditActions -= EditorActions;
            StoryMakerEvents.EditExpressions -= EditExpressions;
            StoryMakerEvents.OnLoadFilm -= OnLoadFilm;
            StoryMakerEvents.EnableStoryEdition -= EnableStoryEdition;
        }

        void OnLoadFilm() {
            storyName.text = Data.Instance.scenesData.currentFilmData.name;
        }

        void Init() {
            tabs.Init(OnTabClicked);
        }

        public void CloseActionsAndEmojis() {
            actionUI.SetOn(false);
            emojisUI.SetOn(false);
        }

        void EditorActions(string id) {
            actionUI.SetCharacterId(id);
            actionUI.SetOn(true);
            emojisUI.SetOn(false);
        }

        void EditExpressions(string id) {
            emojisUI.SetCharacterId(id);
            actionUI.SetOn(false);
            emojisUI.SetOn(true);
        }

        void OnTabClicked(int id) {
            actionUI.SetOn(false);
            emojisUI.SetOn(false);
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
                if (so == null)
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

                Debug.Log("# Adding Id: " + soData.id + " " + soData);


                sdf.AddSO(soData);

                /*if (customizerData != "" && Data.Instance.scenesData.currentFilmData.IsMyStory())
                    StoryMakerEvents.SetNewAvatarCustomization(customizerData);*/

            }
            if (!changesMade)
                OnChangesMade();
        }

        void OnChangesMade() {
            if (!editionEnabled)
                return;
            changesMade = true;
            DoneBtn.gameObject.SetActive(true);
        }

        public void Save() {
            DoneBtn.gameObject.SetActive(false);
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
            DoneBtn.SetActive(true);
            savePanel.SetActive(false);
        }

        void SaveWork() {
            UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.charactersData.thumbSize, (tex) => {
                Data.Instance.scenesData.currentFilmData.thumb = tex;
                Data.Instance.scenesData.currentFilmData.name = storyName.text;
                Data.Instance.scenesData.SaveFilm();
            });            
        }

        void EnableStoryEdition(bool enable) {

            editionEnabled = enable;
            tabs.gameObject.SetActive(enable);
            storyName.interactable = enable;

            if (!enable)
                DoneBtn.gameObject.SetActive(false);

            Invoke(nameof(SetTabState), Time.deltaTime * 10);
        }

        void SetTabState() {
            int id = editionEnabled ? 0 : 4;
            OnTabClicked(id);
        }
    }
}