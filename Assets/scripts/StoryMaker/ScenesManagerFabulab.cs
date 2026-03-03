using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class ScenesManagerFabulab : ScenesManager
    {
        static ScenesManagerFabulab mInstance = null;

        public new List<SceneDataFabulab> Scenes {
            get {
                return scenes;
            }
            set {
                scenes = value;
            }
        }
        [SerializeField] private List<SceneDataFabulab> scenes;

        public static ScenesManagerFabulab Instance {
            get {
                if (mInstance == null) {
                    mInstance = FindObjectOfType<ScenesManagerFabulab>();
                }
                return mInstance;
            }
        }

        void Awake() {

            if (!mInstance)
                mInstance = this;
            else {
                Destroy(this.gameObject);
                return;
            }
            DontDestroyOnLoad(this.gameObject);
        }

        public override void Init() {
            currentSceneId = 1;
            SceneDataFabulab sd = new SceneDataFabulab();
            sd.Init();
            scenes.Add(sd);
        }

        public override void AddNewScene(int _id) {
            Debug.Log("# AddNewScene");
            SceneDataFabulab activeScene = new SceneDataFabulab();
            currentSceneId = _id;
            scenes.Insert(currentSceneId - 1, activeScene);
        }

        public new SceneDataFabulab GetActiveScene() {
            Debug.Log("# count: " + scenes.Count + " currentSceneId: " + currentSceneId);
            return scenes.Count > 0 ? scenes[currentSceneId - 1] : null;
        }
        public new SceneDataFabulab GetNextActiveScene() {
            if (scenes.Count <= currentSceneId)
                return null;
            return scenes[currentSceneId];
        }

        public new void Restart() {
            //currentFDataID = "";
            scenes.Clear();
            Init();
        }

        public override int GetBackground(int id) {
            if (id - 1 >= scenes.Count)
                return -1;
            return scenes[id - 1].bgID;
        }
        public override void DeleteActiveScene() {
            SceneDataFabulab sd = GetActiveScene();
            scenes.Remove(sd);
        }
        
        public override void RemoveScene(int _id) {
            scenes.RemoveAt(currentSceneId - 1);
        }
                
        /*public int GetTotalScenes()
        {
            return Scenes.Count;
        }*/

        public override void AddSceneObjectsToScene(bool next = true) {
            //  print("currentSceneId: " + currentSceneId + "    next " + next);

            // if (currentSceneId == 1)
            //     Events.ResetScenario();


            GetActiveScene()?.DeleteChangedSO(next);
            GetActiveScene()?.AddSceneObjects(next);
        }        
    }
}
