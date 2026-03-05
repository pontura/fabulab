using BoardItems;
using UnityEngine;

namespace UI.MainApp
{
    public class BoardItemManager : MonoBehaviour
    {
        public virtual void Init() { }
        public virtual void AttachItem(ItemInScene item)  {  }
        public virtual void OnStopDrag(ItemInScene item)  {  }
        public virtual void MoveBack(ItemInScene itemSelected)  {  }
        public virtual void MoveUp(ItemInScene itemSelected) {  }

        public void SetInteractableObject()
        {
            Invoke("SetCollidersOff", 0.1f);
        }
        void SetCollidersOff()
        {
            foreach (ItemData comp in transform.GetComponentsInChildren<ItemData>())
                Destroy(((Component)comp));
            foreach (ItemInScene comp in transform.GetComponentsInChildren<ItemInScene>())
                Destroy(((Component)comp));
            foreach (Collider2D comp in transform.GetComponentsInChildren<Collider2D>())
                Destroy(((Component)comp));
            foreach (Rigidbody2D comp in transform.GetComponentsInChildren<Rigidbody2D>())
                Destroy(((Component)comp));

            SetCollidersOn();
            AddItemData();
        }
        void AddItemData()
        {
            ItemData itemData = gameObject.AddComponent<ItemData>();
            itemData.Init();
            itemData.part = BoardItems.Characters.CharacterPartsHelper.parts.BODY;
            ItemInScene iInScene = gameObject.AddComponent<ItemInScene>();
            iInScene.data = itemData;
            print("BoardItemManager AddItemData " + gameObject.name);
            gameObject.tag = "DragItem";
        }
       
        void SetCollidersOn()
        { 
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
        }
       
    }
}
