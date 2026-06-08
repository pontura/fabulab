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
        List<TagData> tags;
        public void Init(List<string> activeTagIds, List<TagData> tags)
        {
            toggles = new List<Toggle>();
            this.tags = tags;
            Utils.RemoveAllChildsIn(container);
            int a = 0;
            foreach (TagData tag in tags)
            {
                Toggle t = Instantiate(tagToggle, container);
                t.GetComponentInChildren<Text>().text = tag.name;
                t.isOn = false;  
                 foreach (string tagID in activeTagIds)
                    if(tag.id == tagID)
                        t.isOn = true;
               // t.onValueChanged.AddListener(isOn => { OnTagClicked(a); });
                toggles.Add(t);
                a++;
            }
        }
        public List<string> GetSelectedTags()
        {
            List<string> selectedTagsId = new List<string>();
            int id = 0;
            foreach( Toggle t in toggles)
            {
                if(t.isOn) selectedTagsId.Add(tags[id].id);
                id++;
            }
            return selectedTagsId;
        }
    }
}