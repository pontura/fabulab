using System.Collections;
using UI;
using UnityEngine;

namespace BoardItems
{
    public class Screenshot : MonoBehaviour
    {
        public Canvas canvas;
        public Camera targetCamera;
        public Renderer targetRenderer;
        [SerializeField] Animator animator;

        Vector2Int targetSize;

        private void Awake()
        {
            Events.Zoom += Zoom;
        }
        private void OnDestroy()
        {
            Events.Zoom -= Zoom;
        }
        public void Zoom(ZoomStates zoom, bool saving = false)
        {
            animator.SetInteger("zoom", (int)zoom);
        }
        
        IEnumerator CaptureRoutine(System.Action<Texture2D> OnDone)
        {
            canvas.enabled = false;
            yield return null; // esperar 1 frame completo
            yield return new WaitForEndOfFrame();

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
                Vector3 sp = targetCamera.WorldToScreenPoint(p);
                if (sp.z < 0) continue;

                min = Vector2.Min(min, sp);
                max = Vector2.Max(max, sp);
            }

            if (min.x >= max.x || min.y >= max.y)
            {
                // el objeto quedó completamente detrás de la cámara / no es visible
                OnDone(null);
                canvas.enabled = true;
                yield break;
            }

            // Tamaño REAL del objeto en pantalla, sin recortar. Este es el tamaño final del texture.
            int fullWidth = Mathf.CeilToInt(max.x - min.x);
            int fullHeight = Mathf.CeilToInt(max.y - min.y);

            // Rect recortado a lo que ReadPixels puede leer de verdad (el buffer de pantalla)
            float xMin = Mathf.Clamp(min.x, 0, Screen.width);
            float yMin = Mathf.Clamp(min.y, 0, Screen.height);
            float xMax = Mathf.Clamp(max.x, 0, Screen.width);
            float yMax = Mathf.Clamp(max.y, 0, Screen.height);

            int captureWidth = Mathf.Max(0, Mathf.RoundToInt(xMax - xMin));
            int captureHeight = Mathf.Max(0, Mathf.RoundToInt(yMax - yMin));

            // Textura final: siempre del tamaño completo del objeto, rellena de negro
            Texture2D texture = new Texture2D(fullWidth, fullHeight, TextureFormat.RGB24, false);
            Color[] black = new Color[fullWidth * fullHeight];
            for (int i = 0; i < black.Length; i++) black[i] = Color.black;
            texture.SetPixels(black);

            if (captureWidth > 0 && captureHeight > 0)
            {
                Rect rect = new Rect(xMin, yMin, captureWidth, captureHeight);
                Texture2D capture = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
                capture.ReadPixels(rect, 0, 0);
                capture.Apply();

                // dónde va, dentro de la textura final, la parte que sí se pudo leer
                int offsetX = Mathf.RoundToInt(xMin - min.x);
                int offsetY = Mathf.RoundToInt(yMin - min.y);

                texture.SetPixels(offsetX, offsetY, captureWidth, captureHeight, capture.GetPixels());
                Destroy(capture);
            }

            texture.Apply();

            if (!targetSize.Equals(Vector2Int.zero))
            {
                texture = TextureUtils.GPUScaleTexture(texture, targetSize.x, targetSize.y);
                targetSize = Vector2Int.zero;
            }

            OnDone(texture);
            yield return new WaitForEndOfFrame();

            canvas.enabled = true;
        }
        IEnumerator CaptureRoutine_OLD(System.Action<Texture2D> OnDone)
        {
            canvas.enabled = (false);
            yield return null; // esperar 1 frame completo
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
                Vector3 sp = targetCamera.WorldToScreenPoint(p);
                if (sp.z < 0) continue;

                min = Vector2.Min(min, sp);
                max = Vector2.Max(max, sp);
            }

            float xMin = Mathf.Clamp(min.x, 0, Screen.width);
            float yMin = Mathf.Clamp(min.y, 0, Screen.height);
            float xMax = Mathf.Clamp(max.x, 0, Screen.width);
            float yMax = Mathf.Clamp(max.y, 0, Screen.height);

            float width = xMax - xMin;
            float height = yMax - yMin;

            //if (width <= 0 || height <= 0)
            //{
            //    OnDone(null);
            //    canvas.gameObject.SetActive(true);
            //    yield break;
            //}

            Rect rect = new Rect(xMin, yMin, width, height);

            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();

            // Guardar archivo local
            // byte[] bytes = texture.EncodeToPNG();
            // System.IO.File.WriteAllBytes(Application.dataPath + "/screenshot.png", bytes);
            // Debug.Log("Screenshot guardado en: " + Application.dataPath + "/screenshot.png");

            if (!targetSize.Equals(Vector2Int.zero)){
                texture = TextureUtils.GPUScaleTexture(texture, targetSize.x,targetSize.y);
                targetSize = Vector2Int.zero;
            }


            OnDone(texture);
            yield return new WaitForEndOfFrame();

            canvas.enabled = (true);
        }

        public void TakeShot(Vector2Int size, System.Action<Texture2D> OnDone)
        {
            Debug.Log("TAKE Screenshot");
            targetSize = size;
            StartCoroutine(CaptureRoutine(OnDone));
        }


    }

}