using UnityEngine;
using UnityEngine.UI;

namespace StoryPlayer
{
    // Shows on screen the texture the story camera renders into (CameraRegionRenderer), as big as it fits
    // keeping its aspect. Without it the camera draws only into the texture and the screen shows nothing.
    // It builds its own canvas, below the progress bar
    public class StoryPlayerScreen : MonoBehaviour
    {
        [SerializeField] CameraRegionRenderer regionRenderer;
        [SerializeField] Color backgroundColor = new Color32(7, 96, 150, 255);

        RawImage view;
        AspectRatioFitter fitter;
        Image background;

        void Start() {
            if (regionRenderer == null)
                regionRenderer = FindFirstObjectByType<CameraRegionRenderer>();
            Build();
        }

        void LateUpdate() {
            RenderTexture texture = regionRenderer != null && regionRenderer.isActiveAndEnabled ? regionRenderer.Target : null;
            // No texture: the camera draws to the screen, nothing to cover it with
            bool visible = texture != null;
            if (background.gameObject.activeSelf != visible)
                background.gameObject.SetActive(visible);
            if (!visible)
                return;
            if (view.texture != texture) {
                view.texture = texture;
                fitter.aspectRatio = (float)texture.width / texture.height;
            }
        }

        void Build() {
            GameObject canvasObject = new GameObject("StoryPlayerScreen", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(canvasObject.transform, false);
            background = backgroundObject.GetComponent<Image>();
            background.color = backgroundColor;
            background.raycastTarget = false;
            Stretch((RectTransform)backgroundObject.transform);

            GameObject viewObject = new GameObject("View", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            viewObject.transform.SetParent(backgroundObject.transform, false);
            view = viewObject.GetComponent<RawImage>();
            view.raycastTarget = false;
            Stretch((RectTransform)viewObject.transform);
            fitter = viewObject.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1;

            backgroundObject.SetActive(false);
        }

        static void Stretch(RectTransform rect) {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
