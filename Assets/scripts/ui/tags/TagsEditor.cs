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
        public void Init(List<string> activeTagIds)
        {
            toggles = new List<Toggle>();
            Utils.RemoveAllChildsIn(container);
            int a = 0;
            foreach (TagData tag in Data.Instance.tagsManager.Tags)
            {
                Toggle t = Instantiate(tagToggle, container);
                t.GetComponentInChildren<Text>().text = tag.name;
                t.isOn = false;
                if(activeTagIds!=null)
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
                if(t.isOn) selectedTagsId.Add(Data.Instance.tagsManager.Tags[id].id);
                id++;
            }
            return selectedTagsId;
        }
    }
}