using System.Collections;
using System.Collections.Generic;
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
            Debug.Log("& Init");
            currentSceneId = 1;
            SceneDataFabulab sd = new SceneDataFabulab();
            sd.Init();
            scenes = new();
            scenes.Add(sd);
        }

        public override void AddNewScene(int _id) {
            Debug.Log("# AddNewScene");
            SceneDataFabulab activeScene = new SceneDataFabulab();
            activeScene.Init();
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

        public override string GetBackground(int id) {
            if (id - 1 >= scenes.Count)
                return "";
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

        public override void AddSceneObjectsToScene(int lastSceneID=-1) {
            //  print("currentSceneId: " + currentSceneId + "    next " + next);

            // if (currentSceneId == 1)
            //     Events.ResetScenario();
            
            if (lastSceneID == currentSceneId)
                lastSceneID = -1;
            Debug.Log("& LastScene = " + lastSceneID);
            GetActiveScene()?.DeleteChangedSO(lastSceneID);
            GetActiveScene()?.AddSceneObjects(lastSceneID);
        }      
        
        public string GetSerialized() {
            string json = "[";
            for(int i = 0; i < scenes.Count; i++) {
                json += "{\"" + nameof(SceneData.bgID) + "\":\"" + scenes[i].bgID + "\",";
                json += "\"" + nameof(SceneDataFabulab.transition) + "\":\"" + scenes[i].transition + "\",";
                json += "\"" + nameof(SceneDataFabulab.duration) + "\":\"" + scenes[i].duration + "\",";
                List<SceneElement> elements = scenes[i].GetScenesElements();
                if (elements.Count > 0)
                    json += "\"scenesElements\":[";
                for(int j = 0; j < elements.Count; j++) {
                    if (elements[j].type == SceneElementType.AVATAR) {
                        SceneElementAvatar sea = (SceneElementAvatar)elements[j];
                        json += JsonUtility.ToJson(sea);
                    } else if (elements[j].type == SceneElementType.WORD_BALLOON || elements[j].type == SceneElementType.WORD_BOX) {
                        SceneElementTextInput sea = (SceneElementTextInput)elements[j];
                        json += JsonUtility.ToJson(sea);
                    } else { 
                        json += JsonUtility.ToJson(elements[j]);
                    }
                    if (j < elements.Count - 1) json += ",";
                }
                if (elements.Count > 0)
                    json += "]";
                json += "}";
                if (i < scenes.Count-1) json += ",";
            }
            json += "]";
            return json;
        }
    }
}
