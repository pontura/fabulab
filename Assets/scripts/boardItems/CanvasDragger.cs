using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardItems.UI
{
    public class CanvasDragger : MonoBehaviour
    {
        bool dragging;
        GameObject container;
        Items items;
        public Camera cam;
        Vector3 offset;

        void Start()
        {
            Events.OnCanvasDragger += OnCanvasDragger;
            items = GetComponent<Items>();
        }
        private void OnDestroy()
        {
            Events.OnCanvasDragger -= OnCanvasDragger;
        }
        void OnCanvasDragger(bool dragging)
        {
            this.dragging = dragging;
            if (dragging)
                InitDrag();
            else
                EndDrag();
        }
        void EndDrag()
        {
            foreach (ItemInScene item in items.all)
            {
                if (item != null && item.IsBeingUse())
                {
                    item.transform.SetParent(items.container.transform);
                    item.data.position = item.transform.position;
                }
            }
            Destroy(container);
        }
        void InitDrag()
        {
            container = Instantiate(new GameObject());
            container.transform.SetParent(transform);
            container.transform.position = Vector3.zero;
            offset = cam.ScreenToWorldPoint(Input.mousePosition);
            foreach (ItemInScene item in items.all)
            {
                if (item.IsBeingUse())
                    item.transform.SetParent(container.transform);
            }
        }
        void Update()
        {
            if (!dragging) return;
            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition) - offset;
            pos.z = 0;
            container.transform.position = pos;
        }
    }

}