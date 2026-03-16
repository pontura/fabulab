using BoardItems;
using BoardItems.BoardData;
using BoardItems.Characters;
using System.Collections;
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
        System.Action<BoardItemManager> OnDone;
        public void SetInteractableObject(System.Action<BoardItemManager> OnDone)
        {
            this.OnDone = OnDone;             
            StartCoroutine(SetCollidersOff());
        }
        IEnumerator SetCollidersOff()
        {
            yield return new WaitForSeconds(0.2f);
            SetCollidersOn();
            yield return new WaitForSeconds(0.2f);

            foreach (ItemData comp in transform.GetComponentsInChildren<ItemData>())
                Destroy(((Component)comp));
            foreach (ItemInScene comp in transform.GetComponentsInChildren<ItemInScene>())
                Destroy(((Component)comp));
            //foreach (Collider2D comp in transform.GetComponentsInChildren<Collider2D>())
            //    Destroy(((Component)comp));
            foreach (Rigidbody2D comp in transform.GetComponentsInChildren<Rigidbody2D>())
                Destroy(((Component)comp));

            BoxCollider2D parentCollider = GetComponent<BoxCollider2D>();
            if (parentCollider == null)
                parentCollider = gameObject.AddComponent<BoxCollider2D>();
            // Convert bounds center to local space
            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
            parentCollider.offset = localCenter;

            // Size of bounds is in world space magnitude, to bring it to local space properly we can divide by the local lossyScale
            Vector3 worldSize = bounds.size;
            Vector3 localScale = transform.lossyScale;

            Vector2 localSize = new Vector2(
                worldSize.x / Mathf.Abs(localScale.x), 
                worldSize.y / Mathf.Abs(localScale.y)
            );
            
            parentCollider.size = localSize;

            AddItemData();
            yield return new WaitForSeconds(0.1f);
            OnDone(this);
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
            Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();

            if (childColliders.Length == 0) return;

            bool firstColliderFound = false;
            
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (Collider2D col in childColliders)
            {
                if (col.gameObject == gameObject) continue; 
                
                firstColliderFound = true;

                if (col is BoxCollider2D box)
                {
                    // Get corners in world space
                    Vector2 extents = box.size / 2f;
                    Vector2[] corners = new Vector2[4]
                    {
                        box.transform.TransformPoint(box.offset + new Vector2(-extents.x, -extents.y)),
                        box.transform.TransformPoint(box.offset + new Vector2(-extents.x, extents.y)),
                        box.transform.TransformPoint(box.offset + new Vector2(extents.x, -extents.y)),
                        box.transform.TransformPoint(box.offset + new Vector2(extents.x, extents.y))
                    };

                    foreach (var pt in corners)
                    {
                        if (pt.x < minX) minX = pt.x;
                        if (pt.y < minY) minY = pt.y;
                        if (pt.x > maxX) maxX = pt.x;
                        if (pt.y > maxY) maxY = pt.y;
                    }
                }
                else if (col is CircleCollider2D circle)
                {
                    Vector2 center = circle.transform.TransformPoint(circle.offset);
                    // Radius scaling might not be uniform, but lossyScale.x is a decent approximation 
                    float radius = circle.radius * Mathf.Max(Mathf.Abs(circle.transform.lossyScale.x), Mathf.Abs(circle.transform.lossyScale.y));
                    
                    if (center.x - radius < minX) minX = center.x - radius;
                    if (center.y - radius < minY) minY = center.y - radius;
                    if (center.x + radius > maxX) maxX = center.x + radius;
                    if (center.y + radius > maxY) maxY = center.y + radius;
                }
                else
                {
                    // Fallback using bounds for PolygonCollider2D / generic colliders
                    Bounds b = col.bounds;
                    if (b.min.x < minX) minX = b.min.x;
                    if (b.min.y < minY) minY = b.min.y;
                    if (b.max.x > maxX) maxX = b.max.x;
                    if (b.max.y > maxY) maxY = b.max.y;
                }
            }

            if(!firstColliderFound) 
            {
                bounds = new Bounds(transform.position, Vector3.zero);
            }
            else
            {
                Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, transform.position.z);
                Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
                bounds = new Bounds(center, size);
            }
        }
        public BodyPart GetBodyPart(CharacterPartsHelper.parts part)
        {
            BoardItems.BodyPart[] all = UIManager.Instance.boardUI.activeBoardItem.GetComponentsInChildren<BoardItems.BodyPart>();
            foreach (BoardItems.BodyPart b in all)
            {
                if (b.part == part) return b;
            }
            return null;
        }

    }
}
