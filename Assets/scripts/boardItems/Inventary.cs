using System.Collections;
using UnityEngine;

namespace BoardItems
{
    public class Inventary : MonoBehaviour
    {
        [SerializeField] ItemPhotoCreator itemPhotoCreator;
        [SerializeField] UI.ItemsUI itemsUI;
        Items items;
        public void Reset()
        {
            StopAllCoroutines();
            if (items != null)
                Utils.RemoveAllChildsIn(items.container);
        }
        public void Init(Items items, GameObject galleryAsset)
        {
            itemsUI.Restart();
            this.items = items;
            GameObject gallery = Instantiate(galleryAsset);
            gallery.transform.SetParent(items.container);
            gallery.transform.localPosition = Vector3.zero;
            StartCoroutine(AddItems(gallery));
        }
        IEnumerator AddItems(GameObject gallery)
        {
            foreach (ItemData itemData in gallery.GetComponentsInChildren<ItemData>())
            {
                yield return new WaitForSeconds(0.05f);
                AddItem(itemData, itemData.transform.position);
            }
            Destroy(gallery.gameObject);
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
            itemsUI.Add(itemData, s);
            itemData.gameObject.SetActive(false);
        }
    }

}