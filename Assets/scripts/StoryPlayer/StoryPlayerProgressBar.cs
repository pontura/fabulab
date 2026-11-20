using UnityEngine;
using UnityEngine.UI;

namespace StoryPlayer
{
    // Bar at the bottom of the screen that shows how much of the story has been played.
    // It builds its own canvas, so the scene needs nothing else
    [RequireComponent(typeof(StoryPlayerController))]
    public class StoryPlayerProgressBar : MonoBehaviour
    {
        [SerializeField] float height = 12;
        [SerializeField] float margin = 0;
        [SerializeField] Color backgroundColor = new Color(1, 1, 1, 0.35f);
        [SerializeField] Color fillColor = Color.white;
        [SerializeField] Vector2 referenceResolution = new Vector2(1080, 1920);

        StoryPlayerController controller;
        GameObject canvasObject;
        RectTransform fill;

        void Awake() {
            controller = GetComponent<StoryPlayerController>();
            Build();
        }

        void OnDestroy() {
            if (canvasObject != null)
                Destroy(canvasObject);
        }

        void LateUpdate() {
            bool visible = controller.IsLoaded;
            if (canvasObject.activeSelf != visible)
                canvasObject.SetActive(visible);
            if (!visible)
                return;
            fill.anchorMax = new Vector2(controller.Progress, 1);
        }

        void Build() {
            canvasObject = new GameObject("StoryPlayerProgressBar", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform background = CreateImage("Background", canvasObject.transform, backgroundColor);
            background.anchorMin = new Vector2(0, 0);
            background.anchorMax = new Vector2(1, 0);
            background.pivot = new Vector2(0.5f, 0);
            background.offsetMin = new Vector2(margin, margin);
            background.offsetMax = new Vector2(-margin, margin + height);

            fill = CreateImage("Fill", background, fillColor);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0, 1);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;

            canvasObject.SetActive(false);
        }

        static RectTransform CreateImage(string name, Transform parent, Color color) {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return (RectTransform)go.transform;
        }
    }
}
