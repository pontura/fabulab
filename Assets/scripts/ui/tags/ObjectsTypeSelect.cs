using System.Collections.Generic;
using BoardItems.BoardData;
using UnityEngine;

namespace UI.MainApp.Home
{
    public class ObjectsTypeSelect : MonoBehaviour
    {
        public TMPro.TMP_Dropdown dropdown;
        System.Action<SObjectData.types> onSelect;
        public void Init(System.Action<SObjectData.types> onSelect)
        {
            this.onSelect = onSelect;
            PopulateDropdown();
        }
        public void SetSelected(SObjectData.types t)
        {
            int id =0;
            int a = 0;
             foreach (SObjectData.types type in System.Enum.GetValues(typeof(SObjectData.types)))
            {
               if(type== t)
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
            foreach (SObjectData.types type in System.Enum.GetValues(typeof(SObjectData.types)))
                options.Add(new TMPro.TMP_Dropdown.OptionData(type.ToString()));
            dropdown.AddOptions(options);
        }
        public void OnChanged()
        {            
            onSelect?.Invoke((SObjectData.types)dropdown.value);
        }
  
    }
}
