using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Yaguar.StoryMaker.Editor;

namespace Yaguar.StoryMaker.Editor
{
    public class InputsManager : MonoBehaviour
    {
        protected bool isDragging;
        protected bool isResize;
        protected bool isRotating;

        protected Vector2 startVector;
        protected float rotGestureWidth = 1f;
        protected float rotAngleMinimum = 1f;

        [SerializeField] protected SceneObjectsManager sceneObjectsManager;

        protected Vector3 startPoint;

        public bool isEnabled;        

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
                        if (isDragging)
                        {
                            sceneObjectsManager.selected.StopDrag();
                            isDragging = false;
                            isResize = false;
                            isRotating = false;
                        }
                    }
                    else if (!isResize && !isRotating && isDragging)
                    {
                        sceneObjectsManager.selected.Move();
                    }
                }
            }
        }


        protected virtual void SelectSO()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.touchCount > 0)
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    return;

            Ray ray = Scenario.Instance.Cam.ScreenPointToRay(Input.mousePosition + Scenario.Instance.Offset);

            RaycastHit2D hit = new RaycastHit2D();
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);
            if (hits.Length > 0)
                hit = hits[0];
            foreach (RaycastHit2D h in hits)
            {
                if (h.collider.gameObject.GetComponent<SceneObject>().GetData() is SOIconData)
                {
                    hit = h;
                    break;
                }
            }

            if (hit.collider != null)
            {
                sceneObjectsManager.selected = hit.collider.gameObject.GetComponent<SceneObject>();
                if (sceneObjectsManager.selected != null)
                {
                    isDragging = true;
                    sceneObjectsManager.selected.BeginDrag();
                    sceneObjectsManager.avatar = sceneObjectsManager.selected.GetComponent<Avatar>();
                    if (sceneObjectsManager.avatar != null)
                    {
                        sceneObjectsManager.selectedAvatar = sceneObjectsManager.avatar;
                        StoryMakerEvents.SetAvatarData(sceneObjectsManager.selectedAvatar.GetData());
                        //Debug.Log("ID: " + sceneObjectsManager.selectedAvatar.GetData().id);
                    }
                    else
                    {
                        StoryMakerEvents.NoneAvatarSelected();
                    }
                }
            }
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

        void EditorRotateSO()
        {
            if (!isRotating)
            {
                startPoint = Input.mousePosition;
                startVector = Vector2.one;
                isRotating = true;
            }
            else
            {
                Vector2 currVector = Input.mousePosition - startPoint;
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

        void EditorResizeSo()
        {
            if (!isResize)
            {
                isResize = true;
                startPoint = Input.mousePosition;
            }
            else
            {

                // Find the difference in the distances between each frame.
                float deltaMagnitudeDiff = (startPoint - Input.mousePosition).magnitude;

                sceneObjectsManager.selected.Resize(deltaMagnitudeDiff);

                startPoint = Input.mousePosition;
            }
        }
    }
}
