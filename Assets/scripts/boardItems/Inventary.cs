using System.Collections;
using System.Linq;
using UI;
using UnityEngine;

namespace BoardItems
{
    public class Inventary : MonoBehaviour
    {
        [SerializeField] ItemPhotoCreator itemPhotoCreator;
        [SerializeField] DragAndDropUI dragAndDropUI;
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
            print("AddItems editMode" + editMode);
            if (editMode)
                StartCoroutine(AddItems(galleryID, gallery, OnAllLoaded));
        }
        IEnumerator AddItems(int galleryID, GameObject gallery, System.Action OnAllLoaded)
        {
            print("AddItems " + galleryID);
            print("AddItems " + galleryID + " count: " + gallery.GetComponentsInChildren<ItemData>().Length);
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

            print("AddItem create sprite");
            itemPhotoCreator.Add(itemData, OnSpriteDone);
        }
        void OnSpriteDone(ItemData itemData, Sprite s)
        {
            print("ADd item  create sprite done");
            dragAndDropUI.Add(itemData, s);
            itemData.gameObject.SetActive(false);
        }
    }

}