using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace BoardItems
{
    public class ItemDragHandler : MonoBehaviour
    {
        public float menuPositionX = 520;
        public Image image;
        ItemInScene item;
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
            if (item == null)
                return;
            Vector2 pos = Camera.main.ScreenToWorldPoint(posInput - offset);
            item.SetPos(pos);

            print(item.gameObject.name + " pos: " + pos);
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