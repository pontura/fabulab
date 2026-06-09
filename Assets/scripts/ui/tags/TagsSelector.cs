using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

namespace UI.MainApp.Home
{
    public class TagsSelector : MonoBehaviour
    {
        public TMPro.TMP_Dropdown dropdown;
        System.Action<string> onTagSelected;
        public void Init(System.Action<string> onTagSelected)
        {
            this.onTagSelected = onTagSelected;
            if (Data.Instance.tagsManager.Tags.Count > 0)
                PopulateDropdown();
            else
                Data.Instance.tagsManager.OnTagsLoaded += PopulateDropdown;
        }
        public void SetTag(string tagID)
        {
            int id =0;
            int a = 0;
            foreach (TagData tag in Data.Instance.tagsManager.Tags)
            {
               if(tag.id == tagID)
                    id = a;
                a++;
            }

            dropdown.SetValueWithoutNotify(id);
        }
        void OnDestroy()
        {
            Data.Instance.tagsManager.OnTagsLoaded -= PopulateDropdown;
        }
        void PopulateDropdown()
        {
            dropdown.ClearOptions();
            List<TMPro.TMP_Dropdown.OptionData> options = new();
            options.Add(new TMPro.TMP_Dropdown.OptionData(Data.Instance.tagsManager.GetNoTagName()));
            foreach (TagData tag in Data.Instance.tagsManager.Tags)
                options.Add(new TMPro.TMP_Dropdown.OptionData(tag.name));
          //  options.Add(new TMPro.TMP_Dropdown.OptionData("Nuevo tag"));
            dropdown.AddOptions(options);
        }
        public void OnChanged()
        {            
            onTagSelected?.Invoke(SelectedTag());
        }
        public string SelectedTag() {
                if(dropdown.value==0)
                    return Data.Instance.tagsManager.GetNoTagName();
                TagData selectedTag = Data.Instance.tagsManager.Tags[dropdown.value-1];
                return selectedTag.id;
        }
  
    }
}
