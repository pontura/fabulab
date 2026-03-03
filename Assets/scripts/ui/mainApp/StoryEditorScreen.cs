using BoardItems;
using Common.UI;
using System.Collections.Generic;
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

        private void Start() {
            tabs.Init(OnTabClicked);
            StoryMakerEvents.OnSaveScene += OnSaveScene;
        }

        void OnDestroy() {
            StoryMakerEvents.OnSaveScene -= OnSaveScene;
        }

        void OnTabClicked(int id) {

            switch (id) {
                case 0:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    characterScreen.Init();
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
            Debug.Log("ACA");
            CreateThumb();
            SceneDataFabulab sdf = ScenesManagerFabulab.Instance.GetActiveScene();
            sdf.Reset();
            SOData bgData = Scenario.Instance.sceneObejctsManager.bgData;
            sdf.bgID = bgData.id;
            foreach (SceneObject so in Scenario.Instance.sceneObejctsManager.sceneObjects) {
                if (so == null)
                    continue;

                SOData soData = so.GetData();

                if (so is AvatarFabulab) {
                    /*AvatarFabulab avatar = so as AvatarFabulab;

                    soData = new SOAvatarFabulabData();
                    soData.customization = avatar.characterManager.characterID;

                    customizerData += avatar.GetData().id + "*";
                    int actionID = avatar.avatarActionsManager.currentAction.id;
                    customizerData += actionID + "*";

                    if (avatar.avatarCustomizer.data.sex == CustomizationData.sexs.BOY)
                        customizerData += "b*";
                    else
                        customizerData += "g*";

                    List<CustomizationData> allStyles = avatar.avatarCustomizer.allStyles;
                    foreach (CustomizationData data in allStyles)
                        customizerData += data.cloth + ":" + data.id + ":" + data.colorID + "*";

                    int expID = avatar.avatarExpresionsManager.currentExpresion.id;
                    customizerData += "ex_" + expID + "*";*/
                }

                sdf.AddSO(soData);

                /*if (customizerData != "" && Data.Instance.scenesData.currentFilmData.IsMyStory())
                    StoryMakerEvents.SetNewAvatarCustomization(customizerData);*/

            }
        }

        public void CreateThumb() {
            if (ScenesManagerFabulab.Instance.currentSceneId == 1) {
                //Debug.Log("Create Thumb");
                
                /*screenshot = Scenario.Instance.Cam.GetComponent<Screenshot>();
                screenshot.TakeShot(CopyTexture);*/
            }
        }
    }
}