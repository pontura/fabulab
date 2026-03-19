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

            StoryMakerEvents.Restart += Restart;
            StoryMakerEvents.ResetScene += ResetScenario;
            //sceneObjects = GetComponentsInChildren<SceneObject>();
            //WaitTillScenesLoaded();
        }

        void OnDestroy()
        {
            StoryMakerEvents.Restart -= Restart;
            StoryMakerEvents.ResetScene -= ResetScenario;
        }        

        public void ResetScenario()
        {
            sceneObejctsManager.ResetScene();
        }

        void Restart()
        {
            sceneObejctsManager.ResetScene();
        }        
    }
}
