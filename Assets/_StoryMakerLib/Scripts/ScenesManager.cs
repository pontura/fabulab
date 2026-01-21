using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class ScenesManager : MonoBehaviour
    {
        static ScenesManager mInstance = null;

        public List<SceneData> scenes;
        public int currentSceneId;
        public string currentFDataID;
        public FilmData currentFilmData;

        [field:SerializeField] public float Keyframe_duration { private set; get; }
        [field: SerializeField] public int MaxKeyframes { private set; get; }

        public static ScenesManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = FindObjectOfType<ScenesManager>();
                }
                return mInstance;
            }
        }

        void Awake()
        {

            if (!mInstance)
                mInstance = this;
            else
            {
                Destroy(this.gameObject);
                return;
            }
            DontDestroyOnLoad(this.gameObject);            
        }

        public virtual void Init()
        {
            currentSceneId = 1;
            SceneData sd = new SceneData();
            sd.Init();
            scenes.Add(sd);
        }
        public int GetBackground(int id)
        {
            if (id - 1 >= scenes.Count)
                return -1;
            return scenes[id - 1].bgID;
        }
        public void DeleteActiveScene()
        {
            SceneData sd = GetActiveScene();
            scenes.Remove(sd);
        }

        public virtual void AddNewScene(int _id)
        {
            SceneData activeScene = new SceneData();
            currentSceneId = _id;
            scenes.Insert(currentSceneId - 1, activeScene);
        }
        public void RemoveScene(int _id)
        {
            scenes.RemoveAt(currentSceneId - 1);
        }
        public SceneData GetActiveScene()
        {
            Debug.Log("# count: " + scenes.Count + " currentSceneId: " + currentSceneId);
            return scenes.Count > 0 ? scenes[currentSceneId - 1] : null;
        }
        public SceneData GetNextActiveScene()
        {
            if (scenes.Count <= currentSceneId)
                return null;
            return scenes[currentSceneId];
        }
        public int GetTotalScenes()
        {
            return scenes.Count;
        }

        public void AddSceneObjectsToScene(bool next = true)
        {
            //  print("currentSceneId: " + currentSceneId + "    next " + next);

            // if (currentSceneId == 1)
            //     Events.ResetScenario();


            GetActiveScene()?.DeleteChangedSO(next);
            GetActiveScene()?.AddSceneObjects(next);
        }

        public void Restart()
        {
            //currentFDataID = "";
            scenes.Clear();
            Init();
        }
    }
}
