using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class SceneObjectsOrderer : MonoBehaviour
    {
        SceneObjectsManager sceneObjectsManager;

        void Start()
        {
            sceneObjectsManager = GetComponent<SceneObjectsManager>();
            StoryMakerEvents.ReorderSceneObjectsInZ += ReorderSceneObjectsInZ;
            Loop();
        }
        private void OnDestroy()
        {
            StoryMakerEvents.ReorderSceneObjectsInZ -= ReorderSceneObjectsInZ;
        }
        void Loop()
        {
            Invoke("Loop", 1);
            ReorderSceneObjectsInZ();
        }
        void ReorderSceneObjectsInZ()
        {
            foreach (SceneObject so in sceneObjectsManager.sceneObjects)
            {
                Vector3 pos = so.transform.localPosition;
                pos.z = so.transform.localPosition.y / 2;
                so.transform.localPosition = pos;
            }
        }
    }
}
