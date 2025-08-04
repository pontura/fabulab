using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BoardItems.UI
{
    public class CanvasZoom : MonoBehaviour
    {
        public Scrollbar scrollBars;
        bool dragging;
        Items items;
        [SerializeField] GameObject container;
        public Camera cam;
        float value;

        void Start()
        {
            items = GetComponent<Items>();
            scrollBars.onValueChanged.AddListener(OnValueChanged);
        }
        public void OnValueChanged(float value)
        {
            this.value = 1 - value / 1.5f;
        }
        public void OnInitScrolling()
        {
            OnValueChanged(scrollBars.value);
            Init();
        }
        public void OnEndSCrolling()
        {
            End();
        }
        void End()
        {
            print("End");
            foreach (ItemInScene item in items.all)
            {
                if (item != null && item.IsBeingUse())
                {
                    item.transform.SetParent(items.container.transform);
                    item.data.scale = item.transform.localScale;
                    item.data.position = item.transform.position;
                }
            }
            Destroy(container);
            dragging = false;
        }
        void Init()
        {
            print("init");
            container = Instantiate(new GameObject());
            container.transform.SetParent(transform);
            container.transform.localScale = new Vector2(value, value);

            Vector3 midPos = Vector3.zero;
            int totalUsedObjects = 0;
            List<float> allZ = new List<float>();
            foreach (ItemInScene item in items.all)
            {
                if (item.IsBeingUse())
                {
                    allZ.Add(item.data.position.z);
                    print(item.data.position.z);
                    midPos += item.data.position;
                    totalUsedObjects++;
                }
            }

            if (totalUsedObjects > 0)
            {
                Vector3 newPos = midPos / totalUsedObjects;
                newPos.z = 0;
                container.transform.position = newPos;
            }

            int id = 0;
            foreach (ItemInScene item in items.all)
            {
                if (item.IsBeingUse())
                {
                    item.transform.SetParent(container.transform);
                    Vector3 pos = item.data.position;
                    item.transform.position = pos;
                    pos = item.transform.localPosition;
                    pos.z = allZ[id];
                    item.transform.localPosition = pos;
                    id++;
                }
            }
            dragging = true;
        }
        void Update()
        {
            if (!dragging) return;
            container.transform.localScale = new Vector3(value, value, value);
        }


    }

}