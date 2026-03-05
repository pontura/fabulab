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

        Bounds bounds;

        public void SetInteractableObject()
        {
            Invoke("SetCollidersOff", 0.1f);
        }
        void SetCollidersOff()
        {
            SetCollidersOn();

            foreach (ItemData comp in transform.GetComponentsInChildren<ItemData>())
                Destroy(((Component)comp));
            foreach (ItemInScene comp in transform.GetComponentsInChildren<ItemInScene>())
                Destroy(((Component)comp));
            foreach (Collider2D comp in transform.GetComponentsInChildren<Collider2D>())
                Destroy(((Component)comp));
            foreach (Rigidbody2D comp in transform.GetComponentsInChildren<Rigidbody2D>())
                Destroy(((Component)comp));

            BoxCollider2D parentCollider = GetComponent<BoxCollider2D>();
            if (parentCollider == null)
                parentCollider = gameObject.AddComponent<BoxCollider2D>();
            // convertir centro a espacio local
            Vector2 localCenter = transform.InverseTransformPoint(bounds.center);

            parentCollider.offset = localCenter;
            parentCollider.size = bounds.size/6;

            AddItemData();
        }
        void AddItemData()
        {
            ItemData itemData = gameObject.AddComponent<ItemData>();
           // itemData.transform.localScale = new Vector3(8, 8, 1);
            itemData.Init();
            itemData.part = BoardItems.Characters.CharacterPartsHelper.parts.BODY;
            ItemInScene iInScene = gameObject.AddComponent<ItemInScene>();
            iInScene.data = itemData;
            print("BoardItemManager AddItemData " + gameObject.name);
            gameObject.tag = "DragItem";
        }
        void SetCollidersOn()
        { 
            Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();

            if (childColliders.Length == 0) return;

            bounds = childColliders[0].bounds;

            foreach (Collider2D col in childColliders)
            {
                if (col.transform == transform) continue; // evitar el collider del parent
                bounds.Encapsulate(col.bounds);
            }
           
        }

    }
}
