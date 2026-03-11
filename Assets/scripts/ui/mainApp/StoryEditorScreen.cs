using BoardItems.Characters;
using Common.UI;
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
        [SerializeField] ActionsUI actionUI;
        [SerializeField] EmojisUI emojisUI;

        [SerializeField] GameObject savePanel;
        [SerializeField] GameObject deleteStoryButton;
        [SerializeField] GameObject saveNewStoryButton;
        [SerializeField] GameObject saveStoryButton;
        [SerializeField] GameObject DoneBtn;
        [SerializeField] bool changesMade;

        private void Start() {
            tabs.Init(OnTabClicked);
            StoryMakerEvents.OnSaveScene += OnSaveScene;
            StoryMakerEvents.EditActions += EditorActions;
            StoryMakerEvents.EditExpressions += EditExpressions;
        }

        void OnDestroy() {
            StoryMakerEvents.OnSaveScene -= OnSaveScene;
            StoryMakerEvents.EditActions -= EditorActions;
            StoryMakerEvents.EditExpressions -= EditExpressions;
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
                Data.Instance.scenesData.SaveFilm();
            });            
        }
    }
}