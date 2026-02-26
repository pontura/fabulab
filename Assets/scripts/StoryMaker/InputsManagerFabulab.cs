using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Yaguar.StoryMaker.Editor;

namespace Yaguar.StoryMaker.Editor
{
    public class InputsManagerFabulab : InputsManager
    {        

        [SerializeField] bool isEnabled;

        private void Start() {
            StoryMakerEvents.EnableInputManager += EnableInputManager;
        }

        private void OnDestroy() {
            StoryMakerEvents.EnableInputManager -= EnableInputManager;
        }

        void EnableInputManager(bool enable) {
            isEnabled = enable;
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


        protected override void SelectSO()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.touchCount > 0)
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    return;

            Ray ray = Scenario.Instance.Cam.ScreenPointToRay(Input.mousePosition + Scenario.Instance.Offset);

            int layer_mask = LayerMask.GetMask("story");

            RaycastHit2D hit = new RaycastHit2D();
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, layerMask:layer_mask);
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

        protected override void EditorRotateSO() {
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

        protected override void EditorResizeSo() {
            if (!isResize) {
                startPoint = Input.mousePosition;
                isResize = true;
            } else {
                float delta = (Input.mousePosition.y - startPoint.y) / (Screen.height * 0.5f);
                float mapped = 1 + delta;
                float sizeFactor = Mathf.Clamp(mapped, 0f, 2f);

                //float sizeFactor = 2 * (Input.mousePosition.y- startPoint.y) / Screen.height;

                sceneObjectsManager.selected.Resize(sizeFactor);

            }
        }
    }
}
