using UnityEngine;
using BoardItems.Characters;
using System.Collections;

namespace BoardItems
{
    public class Screenshot : MonoBehaviour
    {
        public Canvas canvas;
        public Camera targetCamera;
        public Renderer targetRenderer;
        [SerializeField] Animator animator;

        private void Awake()
        {
            Events.Zoom += Zoom;
        }
        private void OnDestroy()
        {
            Events.Zoom -= Zoom;
        }
        public void Zoom(CharacterData.parts part)
        {
            animator.SetInteger("zoom", (int)part);
        }

        IEnumerator CaptureRoutine(System.Action<Texture2D> OnDone)
        {
            canvas.gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();

            Bounds bounds = targetRenderer.bounds;

            // Obtener los 8 puntos del bounding box
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
                Vector3 screenPoint = targetCamera.WorldToScreenPoint(p);

                min = Vector2.Min(min, screenPoint);
                max = Vector2.Max(max, screenPoint);
            }

            Rect rect = new Rect(
                min.x,
                min.y,
                max.x - min.x,
                max.y - min.y
            );

            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();

            // Guardar archivo local
           // byte[] bytes = texture.EncodeToPNG();
           // System.IO.File.WriteAllBytes(Application.dataPath + "/screenshot.png", bytes);
           // Debug.Log("Screenshot guardado en: " + Application.dataPath + "/screenshot.png");


            OnDone(texture);

            canvas.gameObject.SetActive(true);
        }

        public void TakeShot(Vector2Int size, System.Action<Texture2D> OnDone)
        {
            Debug.Log("TAKE Screenshot");
            StartCoroutine(CaptureRoutine(OnDone));
        }


    }

}