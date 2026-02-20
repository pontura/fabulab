using BoardItems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardItems
{
    public class ObjectCreator : MonoBehaviour
    {
        public GameObject[] go_to_create;
        public List<ItemData> all;

        void Start()
        {
            int id = 1;
            foreach (GameObject go in go_to_create)
            {
                AlbumData.CharacterData wData = new AlbumData.CharacterData();
                wData.items = new List<AlbumData.SavedIData>();
                wData.id = id.ToString();
                foreach (ItemData itemData in go.GetComponentsInChildren<ItemData>())
                {
                    AlbumData.SavedIData s = SetItemData(itemData);
                    wData.items.Add(s);
                }
            //    Data.Instance.albumData.pakapakaAlbum.Add(wData);
                id++;
            }
        }
        AlbumData.SavedIData SetItemData(ItemData itemData)
        {
            AlbumData.SavedIData d = new AlbumData.SavedIData();
            d.position = itemData.gameObject.transform.position;
            d.rotation = itemData.gameObject.transform.localEulerAngles;
            d.scale = itemData.gameObject.transform.localScale;
            d.anim = itemData.anim;
            d.color = itemData.colorName;
            d.id = itemData.id;

            return d;
        }
    }

}