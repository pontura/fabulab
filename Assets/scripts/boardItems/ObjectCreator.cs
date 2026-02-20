using BoardItems.BoardData;
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
                CharacterData wData = new CharacterData();
                wData.items = new List<SavedIData>();
                wData.id = id.ToString();
                foreach (ItemData itemData in go.GetComponentsInChildren<ItemData>())
                {
                    SavedIData s = SetItemData(itemData);
                    wData.items.Add(s);
                }
            //    Data.Instance.albumData.pakapakaAlbum.Add(wData);
                id++;
            }
        }
        SavedIData SetItemData(ItemData itemData)
        {
            SavedIData d = new SavedIData();
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