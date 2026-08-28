using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home
{
    public class TagsEditor : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [SerializeField] Toggle tagToggle;
        [SerializeField] List<Toggle> toggles;
        [SerializeField] Transform container;
        List<string> tagIds;
        public void Init(List<string> activeTagIds, string itemType)
        {
            toggles = new List<Toggle>();
            tagIds = new List<string>();
            Utils.RemoveAllChildsIn(container);
            int a = 0;
            foreach (TagData tag in Data.Instance.tagsManager.Tags)
            {
                Debug.Log("#"+tag.name);
                if (tag.itemTypes!= null) {
                    if(!tag.itemTypes.Contains(itemType))
                        continue;
                }
                Toggle t = Instantiate(tagToggle, container);
                t.GetComponentInChildren<Text>().text = tag.name;
                t.isOn = false;
                if(activeTagIds!=null)
                    foreach (string tagID in activeTagIds)
                        if(tag.id == tagID)
                            t.isOn = true;
               // t.onValueChanged.AddListener(isOn => { OnTagClicked(a); });
                toggles.Add(t);
                tagIds.Add(tag.id);
                a++;
            }
        }
        public List<string> GetSelectedTags()
        {
            List<string> selectedTagsId = new List<string>();
            int id = 0;
            foreach( Toggle t in toggles)
            {
                if(t.isOn) selectedTagsId.Add(tagIds[id]);
                id++;
            }
            return selectedTagsId;
        }
    }
}