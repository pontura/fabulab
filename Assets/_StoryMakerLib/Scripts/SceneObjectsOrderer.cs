using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class SceneObjectsOrderer : MonoBehaviour
    {
        [SerializeField] float zFactor = 0.5f;
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
                Vector3 pos;
                if (so.GetComponent<ObjectSignal>() != null)
                    return;
                if (so.GetData() is SODataFixed)
                    continue;
                pos = so.transform.localPosition;
                pos.z = so.transform.localPosition.y * zFactor;
                so.transform.localPosition = pos;
            }
        }
    }
}
