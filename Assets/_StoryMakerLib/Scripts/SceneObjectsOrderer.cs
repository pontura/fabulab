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
                if (so.GetData() is SODataFixed || so.GetData() is SOInputData)
                    continue;
                Vector3 pos = so.transform.localPosition;
                if (so.GetData().force_z != 0) pos.z = so.GetData().force_z;
                else
                    pos.z = so.transform.localPosition.y * zFactor;
                so.transform.localPosition = pos;
            }
        }
    }
}
