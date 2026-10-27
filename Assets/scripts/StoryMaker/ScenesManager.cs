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
            currentSceneId = 1;
            scenes = new();
            scenes.Add(CreateScene());
        }

        // A new empty scene with the default settings
        SceneData CreateScene() {
            SceneData scene = new SceneData();
            scene.duration = Keyframe_default_duration;
            BgLigthtingPalette bglp = Scenario.Instance.sceneObjectsManager.BackgroundLighting.GetDefaultPalette();
            scene.lightingId = bglp.Id;
            scene.lightingValue = bglp.DefaultStep;
            scene.transition = true;
            scene.camData = new CamData();
            return scene;
        }

        // Inserts a keyframe at _id and makes it the active one. It starts as a copy of the previous
        // keyframe without its texts
        public void AddNewScene(int _id) {
            SceneData activeScene = CreateScene();
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
        // The scene of a keyframe (1 based), or null when there is none
        public SceneData GetScene(int id) {
            return id >= 1 && id <= scenes.Count ? scenes[id - 1] : null;
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
            Scenario.Instance.sceneObjectsManager.AddAllSceneObjects(elementsToInstantiate);
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

        // The element of a character or object in the scene closest to the active one (the previous ones first),
        // or null when the story does not have it
        public SceneElement FindClosestElement(string id, SceneElementType type) {
            int active = currentSceneId - 1;
            for (int distance = 0; distance < scenes.Count; distance++) {
                foreach (int index in new[] { active - distance, active + distance }) {
                    if (index < 0 || index >= scenes.Count)
                        continue;
                    SceneElement element = scenes[index].GetScenesElements().Find(e => e.type == type && e.data.id == id);
                    if (element != null)
                        return element;
                }
            }
            return null;
        }

        public bool HasElement(string id, SceneElementType type) {
            return scenes.Any(s => s.GetScenesElements().Any(e => e.type == type && e.data.id == id));
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
                foreach (SceneElement se in scene.GetScenesElements().Where(b => b.data.id == oldId && b.data.itemName == oldItemName)) {
                    se.data.id = newId;
                    se.data.itemName = newItemName;
                }
            }
            MarkChanged();
        }
        public SceneElement GetSOInScene(int sceneID, string soID)
        {
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
