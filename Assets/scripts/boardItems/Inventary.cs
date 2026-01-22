using System.Collections;
using UnityEngine;

namespace BoardItems
{
    public class Inventary : MonoBehaviour
    {
        [SerializeField] ItemPhotoCreator itemPhotoCreator;
        [SerializeField] UI.DragAndDropUI dragAndDropUI;
        Items items;
        public void Reset()
        {
            StopAllCoroutines();
            if (items != null)
                Utils.RemoveAllChildsIn(items.container);
            dragAndDropUI.Reset();
        }
        public void Init(Items items, int galleryID, GameObject galleryAsset, bool editMode, System.Action OnAllLoaded)
        {
            this.items = items;
            GameObject gallery = Instantiate(galleryAsset);
            gallery.transform.SetParent(items.container);
            gallery.transform.localPosition = Vector3.zero;
            if(editMode)
                StartCoroutine(AddItems(galleryID, gallery, OnAllLoaded));
        }
        IEnumerator AddItems(int galleryID, GameObject gallery, System.Action OnAllLoaded)
        {
            foreach (ItemData itemData in gallery.GetComponentsInChildren<ItemData>())
            {
                yield return new WaitForEndOfFrame();
                itemData.galleryID = galleryID;
                AddItem(itemData, itemData.transform.position);
            }
            Destroy(gallery.gameObject);

            if(OnAllLoaded != null)
                OnAllLoaded();
        }
        public void AddItem(ItemData itemData, Vector2 pos)
        {
            itemData.transform.SetParent(items.container);
            //itemData.gameObject.AddComponent<Rigidbody2D>();
            ItemInScene itemInScene = itemData.gameObject.AddComponent<ItemInScene>();
            itemInScene.data = itemData;
            itemData.Init();

            //itemInScene.gameObject.layer = 8;
            //foreach (SpriteRenderer sr in itemInScene.GetComponentsInChildren<SpriteRenderer>())
            //    sr.gameObject.layer = 8; //scene;

            itemPhotoCreator.Add(itemData, OnSpriteDone);
        }
        void OnSpriteDone(ItemData itemData, Sprite s)
        {
            dragAndDropUI.Add(itemData, s);
            itemData.gameObject.SetActive(false);
        }
    }

}