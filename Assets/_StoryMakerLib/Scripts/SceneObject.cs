using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class SceneObject : MonoBehaviour
    {

        [SerializeField] private SOData data;

        private float startPosX, startPosY;
        private bool isBeingHeld;
        private Vector3 originalScale;

        private float MAX_SCALE = 2f;
        private float MIN_SCALE = 0.5f;

        public void SetData(SOData data)
        {

            this.data = data;
        }
        public SOData GetData()
        {
            return data;
        }
        public void Init(SOData data)
        {
            this.data = data;
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            if (box == null)
                box = gameObject.AddComponent<BoxCollider2D>();

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                Vector3 scale = r.transform.localScale;
                //Debug.Log(scale);
                float x = box.size.x;
                float y = box.size.y;
                bool change = false;
                if (box.size.x < r.bounds.size.x)
                {
                    x = r.bounds.size.x / scale.x;
                    //Debug.Log(r.name + " X: " + x);
                    change = true;
                }
                if (box.size.y < r.bounds.size.y)
                {
                    y = r.bounds.size.y / scale.y;
                    //Debug.Log(r.name + " Y: " + y);
                    change = true;
                }

                if (change)
                    box.size = new Vector2(x, y);
            }

            originalScale = Vector3.one;
            OnInit();
        }


        public void BeginDrag()
        {
            StoryMakerEvents.HideSoButtons();
            Vector3 mousePos = Input.mousePosition + Scenario.Instance.Offset;
            Vector3 pos = Scenario.Instance.Cam.ScreenToWorldPoint(mousePos);

            startPosX = pos.x - transform.localPosition.x;
            startPosY = pos.y - transform.localPosition.y;

            isBeingHeld = true;
            //Debug.Log("BeingHeld=" + isBeingHeld);
        }

        public void StopDrag()
        {
            //UpdatePos();
            //Events.ShowSoButtons(Scenario.Instance.cam.WorldToScreenPoint(gameObject.transform.localPosition), data);     
            StoryMakerEvents.ShowSoButtons(Input.mousePosition, data);
            isBeingHeld = false;
            StoryMakerEvents.ReorderSceneObjectsInZ();
        }

        public void Move()
        {
            if (isBeingHeld)
            {
                float lastPosX = data.pos.x;
                UpdatePos();
                gameObject.transform.localPosition = data.pos;

                if (data is SOAvatarData)
                {
                    if (!data.goLeft && lastPosX > data.pos.x)
                    {
                        transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
                        data.goLeft = true;
                    }
                    else if (data.goLeft && lastPosX < data.pos.x)
                    {
                        transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
                        data.goLeft = false;
                    }
                }
            }
        }

        void UpdatePos()
        {
            Vector3 mousePos = Input.mousePosition + Scenario.Instance.Offset;
            data.pos = Scenario.Instance.Cam.ScreenToWorldPoint(mousePos);
            data.pos = new Vector3(data.pos.x - startPosX, data.pos.y - startPosY, 0);
        }

        public void Resize(float factor)
        {

            data.goLeft = false;

            data.size = Mathf.Lerp(data.size, data.size * factor, 0.001f);
            if (data.size < MIN_SCALE)
                data.size = MIN_SCALE;
            else if (data.size > MAX_SCALE)
                data.size = MAX_SCALE;
            transform.localScale = new Vector3(data.size * originalScale.x, data.size * originalScale.y, 1);

        }

        public void Rotate(float z)
        {
            transform.localRotation = Quaternion.Euler(0, 0, z);
            data.rot = z;
        }
        public virtual void OnInit() { }
    }
}
