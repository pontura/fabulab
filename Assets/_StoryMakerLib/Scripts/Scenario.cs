using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class Scenario : MonoBehaviour
    {
        //public SceneObject[] sceneObjects;
        [field:SerializeField] public Camera Cam { private set; get; }

        [field: SerializeField] public Vector3 Offset { private set; get; }


        //public ScenesManager scenesManager;
        [field: SerializeField] public SceneObjectsManager sceneObejctsManager { private set; get; }
        [field: SerializeField] public MovementManager movementManager { private set; get; }


        static Scenario mInstance = null;
        public static Scenario Instance
        {
            get
            {
                return mInstance;
            }
        }
        void Awake()
        {
            if (!mInstance)
                mInstance = this;

            sceneObejctsManager = GetComponent<SceneObjectsManager>();
            movementManager = GetComponent<MovementManager>();
            transform.localPosition = Offset;
        }

        void Start()
        {

            StoryMakerEvents.OnSaveScene += OnSaveScene;
            StoryMakerEvents.Restart += Restart;
            //sceneObjects = GetComponentsInChildren<SceneObject>();
            //WaitTillScenesLoaded();
        }

        void OnDestroy()
        {
            StoryMakerEvents.OnSaveScene -= OnSaveScene;
            StoryMakerEvents.Restart -= Restart;
        }        

        public void ResetScenario()
        {
            sceneObejctsManager.ResetScene();
        }

        void Restart()
        {
            sceneObejctsManager.ResetScene();
        }

        public void OnSaveScene()
        {
            CreateThumb();
            // Data.Instance.scenesData.GetActiveScene().Reset(); ##
            SOData bgData = Scenario.Instance.sceneObejctsManager.bgData;
            // Data.Instance.scenesData.GetActiveScene().bgID = bgData.id; ##
            foreach (SceneObject so in Scenario.Instance.sceneObejctsManager.sceneObjects)
            {
                string customizerData = "";
                /* if (so is Avatar)##
                {
                    Avatar avatar = so as Avatar;
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
                    customizerData += "ex_" + expID + "*";
                }

                if (so != null)
                {
                    SOData soData = so.GetData();
                    if (soData != null)
                        Data.Instance.scenesData.GetActiveScene().AddSO(soData, customizerData);
                }

                if (customizerData != "" && Data.Instance.scenesData.currentFilmData.IsMyStory())
                    Events.SetNewCustomization(customizerData);## */

            }
        }

        public void CreateThumb()
        {
          /*  if (Data.Instance.scenesData.currentSceneId == 1) ##
            {
                //Debug.Log("Create Thumb");
                screenshot = Scenario.Instance.cam.GetComponent<Screenshot>();
                screenshot.TakeShot(CopyTexture);
            } ##
          */
        }

        void CopyTexture(Texture2D tex)
        {
            // Data.Instance.scenesData.currentFilmData.thumb = tex; ##
            //Debug.Log("Copy thumb");
        }
    }
}
