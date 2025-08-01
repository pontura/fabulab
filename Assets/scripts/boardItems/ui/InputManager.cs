using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoardItems.UI
{
    public class InputManager : MonoBehaviour
    {
        public ItemDragHandler itemDragHandler;
        public ToolsMenu toolsMenu;
        public states state;
        Vector3 clickPosition;
        public Items items;

        public enum states
        {
            IDLE,
            DRAGGING,
            TOOLS,
            ROTATING,
            SCALING,
            CANVAS_DRAGGING
        }

        void Start()
        {
            Events.OnStartDrag += OnStartDrag;
        }
        void OnDestroy()
        {
            Events.OnStartDrag -= OnStartDrag;
        }
        void OnStartDrag(ItemInScene item, Vector3 originalPosition)
        {
            AudioManager.Instance.sfxManager.PlayTransp("get", -2);
            state = states.DRAGGING;
            itemDragHandler.OnStartDrag(item, originalPosition);
        }
        float lastMouseX;
        void Update()
        {
#if UNITY_ANDROID || UNITY_IOS
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
                        if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Ended)
                        {
                            Events.OnCanvasDragger(false);
                            state = states.IDLE;
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
                            itemDragHandler.StopDragging(centerPos);
                            if (Input.touches[0].position.x < Screen.width * 0.6f && Vector3.Distance(clickPosition, centerPos) < 4 && !IsPointerOverUIObject())
                                OpenTools();
                            else
                                AudioManager.Instance.sfxManager.PlayTransp("drop", -2);
                        }
                        else
                            itemDragHandler.UpdateDrag(centerPos);
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
                        Events.OnCanvasDragger(false);
                        state = states.IDLE;
                    }
                    break;
                case states.DRAGGING:
                    if (Input.GetMouseButtonUp(0))
                    {
                        state = states.IDLE;
                        itemDragHandler.StopDragging(Input.mousePosition);
                        if (Input.mousePosition.x < Screen.width * 0.6f && Vector3.Distance(clickPosition, Input.mousePosition) < 4 && !IsPointerOverUIObject())
                            OpenTools();
                        else
                            AudioManager.Instance.sfxManager.PlayTransp("drop", -2);
                    }
                    else
                        itemDragHandler.UpdateDrag(Input.mousePosition);
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
            AudioManager.Instance.sfxManager.StopLoop();
            clickPosition = Input.mousePosition;
            switch (state)
            {
                case states.SCALING:
                case states.ROTATING:
                    state = states.IDLE;
                    break;
                case states.IDLE:
                    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hit = Physics2D.Raycast(pos, Vector3.forward);
                    if (hit.collider == null)
                    {
                        if (Input.mousePosition.x < Screen.width * 0.6f)
                        {
                            if (!IsPointerOverUIObject())
                            {
                                Events.OnCanvasDragger(true);
                                state = states.CANVAS_DRAGGING;
                            }
                        }
                    }
                    else if (hit.transform.gameObject.tag == "DragItem")
                    {
                        if (state == states.TOOLS)
                            OnCloseTools(states.IDLE);

                        ItemInScene itemInScene = hit.transform.gameObject.GetComponent<ItemInScene>();
                        Vector3 _offset = Camera.main.WorldToScreenPoint(hit.transform.position);
                        OnStartDrag(itemInScene, _offset);
                        UIManager.Instance.boardUI.items.StartDrag(itemInScene);
                    }
                    break;
            }
        }
        void OpenTools()
        {
            state = states.TOOLS;
            Vector3 pos = Input.mousePosition;
            toolsMenu.Init(items.GetItemSelected().data, pos, this);
        }
        public void OnCloseTools(states _state = states.IDLE)
        {
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