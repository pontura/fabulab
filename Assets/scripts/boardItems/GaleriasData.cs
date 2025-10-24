using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BoardItems
{
    public class GaleriasData : MonoBehaviour
    {

        [Serializable]
        public class GalleryData
        {
            public int id;
            public GameObject gallery;
            public PalettesManager.colorNames colorUI;
            public PalettesManager.colorNames[] colors;
        }

        public List<GalleryData> all;
        Dictionary<int, List<ItemData>> itemsPerGallery;

        private void Awake()
        {
            itemsPerGallery = new Dictionary<int, List<ItemData>>();
        }

        public bool ExistGallery(int id)
        {
            return all.Find(x => x.id == id) != null;
        }

        public GalleryData GetGallery(int id)
        {
            return all.Find(x => x.id == id);
        }
        public ItemData GetItem(int galleryID, int itemID)
        {
            Debug.Log(galleryID + "_" + itemID);
            if (!itemsPerGallery.ContainsKey(galleryID))
            {
                GalleryData gData = all.Find(x => x.id == galleryID);
                ItemData[] itemsData = gData.gallery.GetComponentsInChildren<ItemData>();
                List<ItemData> items = new List<ItemData>();
                foreach (ItemData id in itemsData)
                    items.Add(id);
                //    if (ib.itemData.sprite == null)
                //        ib.itemData.sprite = ib.image.sprite;
                //    items.Add(ib.itemData);
                //}
                //Debug.Log("itemsCount: " + items.Count);
                itemsPerGallery.Add(galleryID, items);
            }
            return itemsPerGallery[galleryID].Find(x => x.id == itemID);
        }

        public PalettesManager.colorNames GetGalleryColorUI(int id)
        {
            //Debug.Log("ID: " + id);
            return all.Find(x => x.id == id).colorUI;
        }
    }

}