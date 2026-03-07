using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI;
namespace BoardItems
{
    public class ItemDragHandler : MonoBehaviour
    {
        public float menuPositionX = 520;
        public Image image;
        ItemInScene item;
        GameObject target;
        Vector3 offset;

        private void Start()
        {
            gameObject.SetActive(false);
        }
        public void OnStartDrag(ItemInScene item, Vector3 originalPosition)
        {
            this.item = item;
            gameObject.SetActive(true);
            offset = Input.mousePosition - originalPosition;
            transform.position = Input.mousePosition - offset;
        }
        public void OnStartDragContainer(GameObject target, Vector3 originalPosition)
        {
            item = null;
            this.target = target;
            gameObject.SetActive(true);
            offset = Input.mousePosition - originalPosition;
            transform.position = Input.mousePosition - offset;
        }
        public void StopDragging(Vector3 posInput)
        {
            bool overSomething = IsPointerOverUIObject();
            //if (overSomething)
            //    Events.ItemBackToMenu(item);
            //else
            //{
            Vector3 pos = Camera.main.ScreenToWorldPoint(posInput - offset);
            Events.OnStopDrag(item, pos);
            //}
            gameObject.SetActive(false);
            item = null;
        }
        public void UpdateDrag(Vector3 posInput)
        {
            if (item == null && target == null)
                return;
            if (item != null)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(posInput - offset);
                pos.z = item.data.position.z;
                SetPos(item.gameObject, pos, UIManager.Instance.boardUI.snap);
            }
            else if (target != null)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(posInput - offset);
                pos.z = -1;
                SetPos(target, pos, UIManager.Instance.boardUI.snap);
            }

        }
        public void SetPos(GameObject item, Vector3 pos, bool snap = false)
        {
            float snapGride = Data.Instance.settings.snapGride;
            if (snap)
            {
                SpriteRenderer sr = item.GetComponentInChildren<SpriteRenderer>();

                Vector3 size = sr.bounds.size;

                // calcular borde inferior izquierdo
                float left = pos.x - size.x / 2f;
                float bottom = pos.y - size.y / 2f;

                // snapear ese borde
                float snappedLeft = Mathf.Round(left / snapGride) * snapGride;
                float snappedBottom = Mathf.Round(bottom / snapGride) * snapGride;

                // reconstruir posición
                pos.x = snappedLeft + size.x / 2f;
                pos.y = snappedBottom + size.y / 2f;

                pos.x = (float)System.Math.Round(pos.x, 4);
                pos.y = (float)System.Math.Round(pos.y, 4);
            }
            item.transform.position = new Vector3(pos.x, pos.y, item.transform.position.z);
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