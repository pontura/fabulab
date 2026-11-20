using UnityEngine;

namespace StoryPlayer
{
    // Renders only a region of what a camera films into a RenderTexture, moved in Y by an offset.
    // The camera keeps working as always (the story playback moves it and changes its zoom): every frame,
    // after those changes, this narrows its projection to the region. The region uses the full resolution
    // of the texture (it is not a crop of a bigger render).
    //
    // region: normalized rect (0..1) of the full frame of the camera, from its bottom-left corner. The full
    // frame is the one the camera would show with referenceAspect.
    // offsetY: world units the region moves up (negative: down), independent of the zoom.
    // To avoid stretching, the aspect of the texture must be referenceAspect * region.width / region.height.
    [RequireComponent(typeof(Camera))]
    public class CameraRegionRenderer : MonoBehaviour
    {
        [SerializeField] RenderTexture target;
        [SerializeField] Rect region = new Rect(0, 0, 1, 1);
        [SerializeField] float offsetY;
        [Tooltip("Aspect (width / height) of the full frame the region is taken from. 0: the aspect of the screen")]
        [SerializeField] float referenceAspect = 16f / 9f;

        Camera cam;

        public RenderTexture Target {
            get => target;
            set { target = value; Apply(); }
        }

        public Rect Region {
            get => region;
            set { region = value; Apply(); }
        }

        public float OffsetY {
            get => offsetY;
            set { offsetY = value; Apply(); }
        }

        void Awake() {
            cam = GetComponent<Camera>();
        }

        void OnEnable() {
            Apply();
        }

        void OnDisable() {
            if (cam == null)
                return;
            cam.targetTexture = null;
            cam.ResetProjectionMatrix();
            cam.ResetAspect();
        }

        // After Update and the coroutines, where the playback moves the camera and changes its zoom
        void LateUpdate() {
            Apply();
        }

        void Apply() {
            if (cam == null || !isActiveAndEnabled)
                return;
            if (cam.targetTexture != target)
                cam.targetTexture = target;

            // The projection of the full frame, with the current zoom of the camera
            cam.ResetProjectionMatrix();
            cam.aspect = FrameAspect();
            Matrix4x4 frame = cam.projectionMatrix;

            float width = Mathf.Max(region.width, 0.0001f);
            float height = Mathf.Max(region.height, 0.0001f);
            // Center of the region in normalized device coordinates (-1..1)
            float centerX = (region.x + width * 0.5f) * 2f - 1f;
            float centerY = (region.y + height * 0.5f) * 2f - 1f;

            // Maps the region to the whole texture: ndc' = (ndc - center) / size, in clip space
            Matrix4x4 crop = Matrix4x4.identity;
            crop.m00 = 1f / width;
            crop.m03 = -centerX / width;
            crop.m11 = 1f / height;
            crop.m13 = -centerY / height;

            // Moves what is filmed down, so the region shows offsetY units higher
            Matrix4x4 offset = Matrix4x4.Translate(new Vector3(0, -offsetY, 0));

            cam.projectionMatrix = crop * frame * offset;
        }

        float FrameAspect() {
            if (referenceAspect > 0)
                return referenceAspect;
            return Screen.height > 0 ? (float)Screen.width / Screen.height : 1f;
        }

        void OnValidate() {
            if (cam != null)
                Apply();
            if (target != null && region.height > 0 && region.width > 0) {
                float regionAspect = (referenceAspect > 0 ? referenceAspect : 16f / 9f) * region.width / region.height;
                float textureAspect = (float)target.width / target.height;
                if (Mathf.Abs(regionAspect - textureAspect) > 0.01f)
                    Debug.LogWarning("[CameraRegionRenderer] The region (aspect " + regionAspect.ToString("0.###")
                        + ") will be stretched into the texture (aspect " + textureAspect.ToString("0.###") + ")", this);
            }
        }

        // The region in the scene view (orthographic camera)
        void OnDrawGizmosSelected() {
            Camera c = cam != null ? cam : GetComponent<Camera>();
            if (c == null || !c.orthographic)
                return;
            float halfHeight = c.orthographicSize;
            float halfWidth = halfHeight * FrameAspect();
            Vector3 origin = c.transform.position + c.transform.forward * c.nearClipPlane;
            Vector3 right = c.transform.right;
            Vector3 up = c.transform.up;

            Gizmos.color = Color.gray;
            DrawRect(origin, right, up, -halfWidth, -halfHeight, halfWidth * 2, halfHeight * 2);
            Gizmos.color = Color.yellow;
            DrawRect(origin, right, up,
                -halfWidth + halfWidth * 2 * region.x,
                -halfHeight + halfHeight * 2 * region.y + offsetY,
                halfWidth * 2 * region.width,
                halfHeight * 2 * region.height);
        }

        static void DrawRect(Vector3 origin, Vector3 right, Vector3 up, float x, float y, float width, float height) {
            Vector3 a = origin + right * x + up * y;
            Vector3 b = a + right * width;
            Vector3 c = b + up * height;
            Vector3 d = a + up * height;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
    }
}
