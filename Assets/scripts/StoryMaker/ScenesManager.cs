using BoardItems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class ScenesManager : MonoBehaviour
    {
        static ScenesManager mInstance = null;

        public List<SceneData> Scenes {
            get {
                return scenes;
            }
            set {
                scenes = value;
            }
        }
        [SerializeField] private List<SceneData> scenes;

        public int currentSceneId;
        public string currentFDataID;
        public FilmData currentFilmData;

        [field: SerializeField] public float Keyframe_default_duration { private set; get; }
        [field: SerializeField] public int MaxKeyframes { private set; get; }

        public static ScenesManager Instance {
            get {
                if (mInstance == null) {
                    mInstance = FindObjectOfType<ScenesManager>();
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

        public void Init() {
            Debug.Log("& Init");
            currentSceneId = 1;
            SceneData sd = new SceneData();
            sd.Init();
            scenes = new();
            scenes.Add(sd);
        }

        // Inserts a keyframe at _id and makes it the active one. It starts as a copy of the previous
        // keyframe without its texts
        public void AddNewScene(int _id) {
            Debug.Log("# AddNewScene");
            SceneData activeScene = new SceneData();
            activeScene.Init();
            currentSceneId = _id;
            scenes.Insert(currentSceneId - 1, activeScene);
            if (currentSceneId - 1 > 0) {
                SceneData prevScene = scenes[currentSceneId - 2];
                activeScene.bgID = prevScene.bgID;
                activeScene.lightingId = prevScene.lightingId;
                activeScene.lightingValue = prevScene.lightingValue;
                activeScene.camData = new CamData();
                activeScene.camData.zoom = prevScene.camData.zoom;
                activeScene.camData.pos = prevScene.camData.pos;
                activeScene.camData.tween = prevScene.camData.tween;
                foreach (SceneElement element in prevScene.GetScenesElements()) {
                    if (element.type == SceneElementType.AVATAR || element.type == SceneElementType.PROP)
                        activeScene.AddSO(element.GetSOData());
                }
            }
            MarkChanged();
        }

        public SceneData GetActiveScene() {
            return scenes.Count > 0 ? scenes[currentSceneId - 1] : null;
        }
        public SceneData GetNextActiveScene() {
            if (scenes.Count <= currentSceneId)
                return null;
            return scenes[currentSceneId];
        }

        public void Restart() {
            scenes.Clear();
            Init();
        }

        public string GetBackground(int id) {
            if (id - 1 >= scenes.Count)
                return "";
            return scenes[id - 1].bgID;
        }

        public void RemoveActiveScene() {
            scenes.RemoveAt(currentSceneId - 1);
            MarkChanged();
        }

        // Tells that the story was edited and has unsaved changes
        public void MarkChanged() {
            StoryMakerEvents.OnStoryChanged();
        }

        public void AddAllObjectsToScene() {
            List<SOData> elementsToInstantiate = new List<SOData>();
            foreach (SceneData sdf in Scenes) {
                elementsToInstantiate.AddRange(sdf.GetScenesElements().Select(p => p.GetSOData()).Where(newItem => !elementsToInstantiate.Any(eItem => eItem.itemName == newItem.itemName)).ToList());
            }
            StoryMakerEvents.AddAllSceneObjects(elementsToInstantiate);
        }

        // Shows the objects of the active scene, turning off the ones of lastSceneID that are no longer in it
        public void SetSceneObjectsIntoScenario(int lastSceneID = -1) {
            if (lastSceneID == currentSceneId)
                lastSceneID = -1;
            GetActiveScene()?.DeleteChangedSO(lastSceneID);
            GetActiveScene()?.AddSceneObjects(lastSceneID);
        }
        
        public string GetSerialized() {
            string json = "[";
            for(int i = 0; i < scenes.Count; i++) {
                json += "{\"" + nameof(SceneData.bgID) + "\":\"" + scenes[i].bgID + "\",";
                json += "\"" + nameof(SceneData.transition) + "\":\"" + scenes[i].transition + "\",";
                json += "\"" + nameof(SceneData.duration) + "\":\"" + scenes[i].duration.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\",";
                json += "\"" + nameof(SceneData.lightingId) + "\":\"" + scenes[i].lightingId + "\",";
                json += "\"" + nameof(SceneData.lightingValue) + "\":\"" + scenes[i].lightingValue + "\",";
                
                if(scenes[i].camData.HasData())
                {                    
                    json += "\"" + "camData_zoom" + "\":\"" + scenes[i].camData.zoom + "\",";
                    json += "\"" + "camData_pos_x" + "\":\"" + scenes[i].camData.pos.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\",";
                    json += "\"" + "camData_pos_y" + "\":\"" + scenes[i].camData.pos.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\",";
                    json += "\"" + "camData_tween" + "\":\"" + scenes[i].camData.tween + "\",";
                }

                List<SceneElement> elements = scenes[i].GetScenesElements();
                if (elements.Count > 0)
                    json += "\"scenesElements\":[";
                for(int j = 0; j < elements.Count; j++) {
                    json += elements[j].ToJson();
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

        // Edits of the active scene. The scenes are the source of truth: every edit made in the
        // scenario is written here at once.

        // Adds an object to the active scene. Avatars and props are also added to the next scenes
        // that share the background
        public void AddToActiveScene(SOData soData) {
            SceneData scene = GetActiveScene();
            if (scene == null)
                return;
            scene.AddSO(soData);
            if (soData is SOAvatarData || soData is SOPropData)
                AddItemToNextScenesSameBG(soData);
            MarkChanged();
        }

        public void UpdateInActiveScene(SOData soData) {
            SceneData scene = GetActiveScene();
            if (scene == null || !scene.UpdateSO(soData))
                return;
            MarkChanged();
        }

        public void RemoveFromActiveScene(SOData soData) {
            SceneData scene = GetActiveScene();
            if (scene == null || !scene.RemoveSO(soData.itemName))
                return;
            MarkChanged();
        }

        // Sets the background of the active scene, and of the next ones that had the same one
        public void SetActiveBackground(string bgID) {
            SceneData scene = GetActiveScene();
            if (scene == null)
                return;
            if (scene.bgID != bgID || string.IsNullOrEmpty(scene.bgID)) {
                ReplaceBG(scene.bgID, bgID);
                scene.bgID = bgID;
            }
            MarkChanged();
        }

        public bool StillExistInOtherScenes(SOData soData) {
            return Scenes.Any(s => s.GetScenesElements().Any(e => e.GetSOData().itemName == soData.itemName));
        }

        void AddItemToNextScenesSameBG(SOData soData) {
            string currentBG = GetActiveScene().bgID;
            for(int i = currentSceneId; i <  Scenes.Count; i++) {
                if (Scenes[i].bgID == currentBG) {
                    Scenes[i].AddSO(soData);
                } else
                    break;
            }
        }
        void ReplaceBG(string oldBG, string newBG) {
            for (int i = currentSceneId; i < Scenes.Count; i++) {
                if (Scenes[i].bgID == oldBG || string.IsNullOrEmpty(Scenes[i].bgID)) {
                    Scenes[i].bgID = newBG;
                } else
                    break;
            }
        }

        public void ReplaceItem(string oldId, string oldItemName, string newId, string newItemName) {
            foreach (SceneData scene in Scenes) {
                Debug.Log($"% ReplaceItem SceneElement id: {oldId} itemName: {oldItemName}");
                foreach (SceneElement se in scene.GetScenesElements().Where(b => b.data.id == oldId && b.data.itemName == oldItemName)) {
                    se.data.id = newId;
                    se.data.itemName = newItemName;
                }
            }
            MarkChanged();
        }
        public SceneElement GetSOInScene(int sceneID, string soID)
        {
            print("OnStopDraw GetSOInScene " + sceneID + " scenes.Count: " + scenes.Count);   
            if(sceneID>=0 && sceneID < scenes.Count)
            {
                SceneData scene = scenes[sceneID];
                foreach (SceneElement se in scene.GetScenesElements().Where(b => b.data.id == soID)) {                    
                    return se;
                }
            }
            return null;
        }
    }    
}
