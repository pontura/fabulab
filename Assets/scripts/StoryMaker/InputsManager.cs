using UnityEngine;
using UnityEngine.EventSystems;

namespace Yaguar.StoryMaker.Editor
{
    public class InputsManager : MonoBehaviour
    {
        [SerializeField] SceneObjectsManager sceneObjectsManager;
        [SerializeField] bool isEnabled;

        bool isDragging;
        bool isResize;
        bool isRotating;

        Vector2 startVector;
        Vector3 startPoint;
        float rotGestureWidth = 1f;
        float rotAngleMinimum = 1f;

        private void Awake() {
            StoryMakerEvents.EnableInputManager += EnableInputManager;
        }

        private void OnDestroy() {
            StoryMakerEvents.EnableInputManager -= EnableInputManager;
        }

        void EnableInputManager(bool enable) {
            isEnabled = enable;
        }

        public void ResetAll()
        {
            isDragging = false;
            isResize = false;
            isRotating = false;
        }

        void Update()
        {
            if (!isEnabled)
                return;

            if (!isDragging)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    SelectSO();
                }
            }
            else
            {

#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftShift)) {
                EditorRotateSO();
            }else if (Input.GetKey(KeyCode.LeftControl)) {
                EditorResizeSo();
            }
#endif

                // If there are two touches on the device...
                if (Input.touchCount == 2)
                {
                    ResizeSo();
                    RotateSO();
                }
                else
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        sceneObjectsManager.selected.StopDrag();
                        sceneObjectsManager.Commit(sceneObjectsManager.selected);
                        AudioManager.Instance.sfxManager.PlayTransp("drop", -2);
                        ResetAll();
                        StoryMakerEvents.UpdateDraw();
                    }
                    else if (!isResize && !isRotating)
                    {
                        sceneObjectsManager.selected.Move();
                    }
                }
            }
        }


        void SelectSO()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.touchCount > 0)
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    return;

            if (sceneObjectsManager.selected is Avatar avatar) {
                avatar.Borders?.Show(false);
            } else if (sceneObjectsManager.selected is Prop prop) {
                prop.Borders?.Show(false);
            }
            sceneObjectsManager.selected = null;
            StoryMakerEvents.HideSoButtons();

            Ray ray = Scenario.Instance.Cam.ScreenPointToRay(Input.mousePosition + Scenario.Instance.Offset);

            int layer_mask = LayerMask.GetMask("story");

            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, layerMask:layer_mask);
            SceneObject hitSO = hits.Length > 0 ? hits[0].collider.GetComponent<SceneObject>() : null;

            if (hitSO == null)
            {
                StoryMakerEvents.NoneItemSelected();
                return;
            }

            sceneObjectsManager.selected = hitSO;
            isDragging = true;
            hitSO.BeginDrag();
            AudioManager.Instance.sfxManager.PlayTransp("get", -2);
        }

        void RotateSO()
        {
            if (!isRotating)
            {
                startVector = Input.GetTouch(1).position - Input.GetTouch(0).position;
                isRotating = startVector.sqrMagnitude > rotGestureWidth * rotGestureWidth;
            }
            else
            {
                Vector2 currVector = Input.GetTouch(1).position - Input.GetTouch(0).position;
                float angleOffset = Vector2.Angle(startVector, currVector);
                Vector3 LR = Vector3.Cross(startVector, currVector);

                if (angleOffset > rotAngleMinimum)
                {
                    if (LR.z > 0)
                    {
                        // Anticlockwise turn equal to angleOffset.
                        sceneObjectsManager.selected.Rotate(angleOffset);
                    }
                    else if (LR.z < 0)
                    {
                        // Clockwise turn equal to angleOffset.
                        sceneObjectsManager.selected.Rotate(-1.0f * angleOffset);
                    }
                }
            }
        }

        void ResizeSo()
        {
            isResize = true;
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

            sceneObjectsManager.selected.Resize(deltaMagnitudeDiff);
        }

        void EditorRotateSO() {
            if (!isRotating) {
                startPoint = Input.mousePosition;
                isRotating = true;
            } else {
                float delta = (Input.mousePosition.y - startPoint.y) / (Screen.height * 0.5f);
                float mapped = 180 * delta;
                float rotateVal = Mathf.Clamp(mapped, -180f, 180f);

                sceneObjectsManager.selected.Rotate(rotateVal);
            }
        }

        void EditorResizeSo() {
            if (!isResize) {
                startPoint = Input.mousePosition;
                isResize = true;
            } else {
                float delta = (Input.mousePosition.y - startPoint.y) / (Screen.height * 0.5f);
                float mapped = 1 + delta;
                float sizeFactor = Mathf.Clamp(mapped, 0f, 2f);

                sceneObjectsManager.selected.Resize(sizeFactor);
            }
        }
    }
}
