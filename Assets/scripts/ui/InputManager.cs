using BoardItems;
using BoardItems.Characters;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] GameObject dragContainer;
        [SerializeField] Camera cam;
        public ItemDragHandler itemDragHandler;
        public ToolsMenu toolsMenu;
        public states state;
        Vector3 clickPosition;
        bool hasDragged = false;
        Vector3 dragOrigin;
        public Items items;
        [SerializeField] CharacterPartsHelper.parts partActive;
        public bool groupOn = false;

        public enum states
        {
            IDLE,
            DRAGGING,
            TOOLS,
            ROTATING,
            SCALING,
            CANVAS_DRAGGING
        }
        private void Start()
        {
            Events.OnBodyPartActive += OnBodyPartActive;
            Events.OnBGColorizerOpen += OnBGColorizerOpen;
            Events.SetGroupToolsOn += SetGroupToolsOn;
            Events.ShowScreen += ShowScreen;
        }
        private void OnDestroy()
        {
            Events.OnBodyPartActive -= OnBodyPartActive;
            Events.OnBGColorizerOpen -= OnBGColorizerOpen;
            Events.SetGroupToolsOn -= SetGroupToolsOn;
            Events.ShowScreen -= ShowScreen;
        }

        bool storyMode;
        private void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.StoryMaker:
                    storyMode = true;
                    break;
                default:
                    storyMode = false;
                    break;
            }
        }
        private void OnBodyPartActive(CharacterPartsHelper.parts p)
        {
            this.partActive = (CharacterPartsHelper.parts)(int)p;
        }
        [SerializeField] BodyPart container;

        void OnStartDrag(ItemInScene item, Vector3 originalPosition)
        {
            AudioManager.Instance.sfxManager.PlayTransp("get", -2);
            state = states.DRAGGING;
            if(groupOn)
            {
                container = item.GetComponentInParent<BodyPart>();
                if(container != null)
                {
                    cam.gameObject.GetComponent<Animator>().enabled = false;
                    Vector3 containerOriginalPosition = cam.WorldToScreenPoint(container.transform.position);
                    itemDragHandler.OnStartDragContainer(container.gameObject, containerOriginalPosition);
                }
                else
                {
                    Events.SetGroupToolsOn(false);
                    itemDragHandler.OnStartDrag(item, originalPosition);
                }
            } else
                itemDragHandler.OnStartDrag(item, originalPosition);
        }
        float lastMouseX;
        void LateUpdate()
        {
            if(partActive == CharacterPartsHelper.parts.none)
                return;

#if UNITY_EDITOR
            //UpdateTouch();
            UpdateMouseInput();
#elif UNITY_ANDROID || UNITY_IOS
            UpdateTouch();   
#else
            UpdateMouseInput();
#endif
        }
        Vector2 originTouch1;
        Vector2 originTouch2;
        float originalDistanceInTouches;
        float originalAngleInTouches;

        void UpdateTouch()
        {

            if (Input.touchCount > 0)
            {
                switch (state)
                {
                    case states.CANVAS_DRAGGING:
                        if (Input.touchCount == 2)
                        {
                            Touch touch1 = Input.GetTouch(0);
                            Touch touch2 = Input.GetTouch(1);

                            if (touch2.phase == TouchPhase.Began)
                            {
                                originalDistanceInTouches = Vector2.Distance(touch1.position, touch2.position);
                            }
                            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                            {
                                cam.gameObject.GetComponent<Animator>().enabled = false;
                                
                                float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                                float distanceDifference = originalDistanceInTouches - currentDistance;
                                if (groupOn)
                                {
                                    GameObject target = container != null ? container.gameObject : dragContainer;
                                    if (target != null)
                                    {
                                        float scaleDiff = distanceDifference * -0.01f;
                                        Vector3 newScale = target.transform.localScale + new Vector3(scaleDiff, scaleDiff, scaleDiff);
                                        if (newScale.x < 0.1f) newScale = new Vector3(0.1f, 0.1f, 0.1f);
                                        target.transform.localScale = newScale;
                                    }
                                }
                                else
                                {
                                    // Adjust orthographic size based on pinch distance (multiplier controls zoom speed)
                                    cam.orthographicSize += distanceDifference * 0.1f;

                                    // Clamp the camera size so it doesn't zoom infinitely or flip upside down
                                    cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 10f, 128f); 
                                }

                                originalDistanceInTouches = currentDistance;
                            }
                        }
                        else if (Input.touchCount == 1)
                        {
                            if (Input.touches[0].phase == TouchPhase.Moved)
                            {
                                if (!storyMode)
                                {
                                    cam.gameObject.GetComponent<Animator>().enabled = false;
                                    Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.touches[0].position);
                                    cam.transform.position += new Vector3(difference.x, difference.y, 0f);
                                }
                            }
                            else if (Input.touches[0].phase == TouchPhase.Ended)
                            {
                               // Events.OnCanvasDragger(false);
                                state = states.IDLE;
                            }
                        }
                        break;
                    case states.DRAGGING:
                        if (Input.touchCount > 1 && Input.touches[1].phase == TouchPhase.Began)
                        {
                            originTouch1 = Input.GetTouch(0).position;
                            originTouch2 = Input.GetTouch(1).position;

                            originalDistanceInTouches = Vector3.Distance(originTouch1, originTouch2);
                            originalAngleInTouches = Utils.GetAngleBetween(originTouch1, originTouch2);
                        }

                        Vector3 centerPos = Input.GetTouch(0).position;
                        if (Input.touchCount > 1 && Input.touches[1].phase == TouchPhase.Moved)
                        {
                            Vector3 nowTouch1 = Input.GetTouch(0).position;
                            Vector3 nowTouch2 = Input.GetTouch(1).position;

                            float scaleValue = Vector3.Distance(nowTouch1, nowTouch2);
                            float angleValue = Utils.GetAngleBetween(nowTouch1, nowTouch2);

                            UIManager.Instance.boardUI.items.Scale((originalDistanceInTouches - scaleValue) * -0.5f);
                            UIManager.Instance.boardUI.items.Rotate((originalAngleInTouches - angleValue) * -2);

                            originalDistanceInTouches = scaleValue;
                            originalAngleInTouches = angleValue;
                        }
                        if (Input.touches[0].phase == TouchPhase.Ended)
                        {
                            state = states.IDLE;

                            if(container!= null)
                                container.OnStopTransformModify();

                            itemDragHandler.StopDragging(centerPos);
                            if (!hasDragged && Vector3.Distance(clickPosition, centerPos) < 4 && !IsPointerOverUIObject())
                                OpenTools();
                            else
                                AudioManager.Instance.sfxManager.PlayTransp("drop", -2);
                        }
                        else
                        {
                            if (Vector3.Distance(clickPosition, centerPos) >= 4)
                                hasDragged = true;

                            if (hasDragged)
                                itemDragHandler.UpdateDrag(centerPos);
                        }
                        break;
                        //case states.SCALING:
                        //    UIManager.Instance.boardUI.items.Scale(lastMouseX - Input.touches[0].position.x);
                        //    break;
                        //case states.ROTATING:
                        //    UIManager.Instance.boardUI.items.Rotate(lastMouseX - Input.touches[0].position.x);
                        //    break;
                }
                if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began)
                    OnPress();
                lastMouseX = Input.mousePosition.x;
            }
        }
        void UpdateMouseInput()
        {
            switch (state)
            {
                case states.CANVAS_DRAGGING:
                    if (Input.GetMouseButtonUp(0))
                    {
                       // Events.OnCanvasDragger(false);
                        state = states.IDLE;
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        if (!storyMode)
                        {
                            cam.gameObject.GetComponent<Animator>().enabled = false;
                            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);                        
                            cam.transform.position += new Vector3(difference.x, difference.y, 0f);
                        }
                    }
                    break;
                case states.DRAGGING:
                    if (Input.GetMouseButtonUp(0))
                    {
                        state = states.IDLE;

                       
                        if (!hasDragged && Vector3.Distance(clickPosition, Input.mousePosition) < 4 && !IsPointerOverUIObject())
                            OpenTools();
                        else
                        {
                            if (container != null)
                            {
                                container.OnStopTransformModify();
                                container = null;
                            }
                            itemDragHandler.StopDragging(Input.mousePosition);
                        }
                    }
                    else
                    {
                        if (Vector3.Distance(clickPosition, Input.mousePosition) >= 4)
                            hasDragged = true;

                        if (hasDragged)
                            itemDragHandler.UpdateDrag(Input.mousePosition);
                    }
                    break;
                case states.SCALING:
                    UIManager.Instance.boardUI.items.Scale(lastMouseX - Input.mousePosition.x);
                    break;
                case states.ROTATING:
                    UIManager.Instance.boardUI.items.Rotate(lastMouseX - Input.mousePosition.x);
                    break;
            }
            if (Input.GetMouseButtonDown(0))
                OnPress();
            lastMouseX = Input.mousePosition.x;
        }

        void OnPress()
        {
            if (IsPointerOverUIObject()) return;
            AudioManager.Instance.sfxManager.StopLoop();
            clickPosition = Input.mousePosition;
            hasDragged = false;
            switch (state)
            {
                case states.SCALING:
                case states.ROTATING:
                    state = states.IDLE;
                    break;
                case states.TOOLS:
                    OnCloseTools(states.IDLE);
                    goto case states.IDLE;
                case states.IDLE:
                    Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hit = Physics2D.Raycast(pos, Vector3.forward);
                   
                    if (hit && hit.transform.gameObject.tag == "DragItem")
                    {
                        ItemInScene itemInScene = hit.transform.gameObject.GetComponent<ItemInScene>();
                        itemInScene.SetTools(false);
                        if (itemInScene.IsBeingUse() && itemInScene.data.part != partActive) return;

                        Vector3 _offset = cam.WorldToScreenPoint(hit.transform.position);

                        OnStartDrag(itemInScene, _offset);
                        UIManager.Instance.boardUI.items.StartDrag(itemInScene);
                    }
                    else
                    {
                        if (!IsPointerOverUIObject())
                        {
                            //  Events.OnCanvasDragger(true);
                            state = states.CANVAS_DRAGGING;
                            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
                        }
                    }
                    break;
            }
        }
        public void OnInitDragging(ItemInScene itemInScene)
        {
            state = states.IDLE;

            Vector3 _offset = cam.ScreenToWorldPoint(Input.mousePosition);
            _offset.z = 0;
            itemInScene.transform.position = _offset;
            itemInScene.GetComponent<ItemData>().position = _offset;

            OnStartDrag(itemInScene, Input.mousePosition);
            UIManager.Instance.boardUI.items.StartDrag(itemInScene);
        }
        [SerializeField] AnimationClip clip;
        [SerializeField] ItemInScene itemSelected;
        void OpenTools()
        {
            state = states.TOOLS;
            Vector3 pos = Input.mousePosition;
            itemSelected = items.GetItemSelected();
            toolsMenu.Init(itemSelected.data, pos, this);
            itemSelected.SetTools(true, clip);
        }
        void OnBGColorizerOpen(bool isOn)
        {
            if (isOn)
            {
                state = states.TOOLS;
                Vector3 pos = Vector2.zero;
                toolsMenu.Init(null, pos, this);
                toolsMenu.SetBGColors();
            }
            else
            {
                OnCloseTools();
            }
        }
        void SetGroupToolsOn(bool isOn)
        {
            print("SetGroupToolsOn " + isOn);
            groupOn = isOn;
            if (!isOn)
                container = null;
        }
        public void OnCloseTools(states _state = states.IDLE)
        {
            if(itemSelected != null)
                itemSelected.SetTools(false);
            toolsMenu.SetOff();
            state = _state;
        }
        public void Delete()
        {
            UIManager.Instance.boardUI.items.Delete();
        }
        public void Rotate()
        {
            AudioManager.Instance.sfxManager.PlayLoop("mod", 0.25f);
            state = states.ROTATING;
        }
        public void Scale()
        {
            AudioManager.Instance.sfxManager.PlayLoop("mod", 0.25f);
            state = states.SCALING;
        }
        public static bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 1;
        }
    }

}