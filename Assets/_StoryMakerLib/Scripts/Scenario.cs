using System;
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

            StoryMakerEvents.ClearScene += ClearScene;
            //sceneObjects = GetComponentsInChildren<SceneObject>();
            //WaitTillScenesLoaded();
        }

        void OnDestroy()
        {
            StoryMakerEvents.ClearScene -= ClearScene;
        }

      

        public void ResetScenario()
        {
            sceneObejctsManager.ResetScene();
        }

        void ClearScene()
        {
            sceneObejctsManager.ClearScene();
        }

        [SerializeField] Renderer targetRenderer;
        public void Screenshot(System.Action<Texture2D> OnDone)
        {
            StartCoroutine(CaptureRoutine(OnDone));
        }
        IEnumerator CaptureRoutine(System.Action<Texture2D> OnDone)
        {
            yield return new WaitForSeconds(0.1f);

            targetRenderer.GetComponent<Animator>().SetInteger("zoom", 9);

            yield return new WaitForEndOfFrame();

            int rtWidth = Screen.width;
            int rtHeight = Screen.height;

            RenderTexture rt = new RenderTexture(rtWidth, rtHeight, 24);
            Cam.targetTexture = rt;

            Cam.Render();

            Bounds bounds = targetRenderer.bounds;

            Vector3[] points = new Vector3[8];

            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            points[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
            points[1] = center + new Vector3(-extents.x, -extents.y, extents.z);
            points[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
            points[3] = center + new Vector3(-extents.x, extents.y, extents.z);
            points[4] = center + new Vector3(extents.x, -extents.y, -extents.z);
            points[5] = center + new Vector3(extents.x, -extents.y, extents.z);
            points[6] = center + new Vector3(extents.x, extents.y, -extents.z);
            points[7] = center + new Vector3(extents.x, extents.y, extents.z);

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            foreach (var p in points)
            {
                Vector3 sp = Cam.WorldToScreenPoint(p);

                if (sp.z < 0) continue;

                min = Vector2.Min(min, sp);
                max = Vector2.Max(max, sp);
            }

            float xMin = Mathf.Clamp(min.x, 0, rtWidth);
            float yMin = Mathf.Clamp(min.y, 0, rtHeight);
            float xMax = Mathf.Clamp(max.x, 0, rtWidth);
            float yMax = Mathf.Clamp(max.y, 0, rtHeight);

            float width = xMax - xMin;
            float height = yMax - yMin;

            if (width <= 0 || height <= 0)
            {
                Cam.targetTexture = null;
                RenderTexture.active = null;
                Destroy(rt);
                OnDone(null);
                yield break;
            }

            Rect rect = new Rect(xMin, yMin, width, height);

            RenderTexture.active = rt;

            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();

            Cam.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            OnDone(texture);
        }
    }
}
