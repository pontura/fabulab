using System.Globalization;
using BoardItems.BoardData;
using UnityEngine;

namespace UI.MainApp.Home
{
    // Filters a list of items by tag and by name
    public class SearchBar : MonoBehaviour
    {
        public TMPro.TMP_InputField searchField;
        public TagsSelector tagsSelector;

        // Called every time the filter changes. Several lists can share the same search bar
        public event System.Action OnChanged;

        bool initialized;

        public void Init()
        {
            if (initialized)
                return;
            initialized = true;
            tagsSelector.Init(tag => OnChanged?.Invoke());
        }

        public void OnSearch()
        {
            OnChanged?.Invoke();
        }

        // True when the item has the selected tag (or none is selected) and its name contains the searched text,
        // ignoring case and accents
        public bool Matches(CharacterMetaData meta)
        {
            string tag = tagsSelector.SelectedTag();
            if (tag != Data.Instance.tagsManager.GetNoTagName() && (meta?.tags == null || !meta.tags.Contains(tag)))
                return false;

            string search = searchField.text.Trim();
            if (search == "")
                return true;
            return meta?.name != null
                && CultureInfo.InvariantCulture.CompareInfo.IndexOf(meta.name, search, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
        }

        public void Maximize()
        {

        }
        public void Minimize()
        {

        }
    }
}
