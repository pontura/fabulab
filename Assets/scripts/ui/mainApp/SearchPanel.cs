using UnityEngine;
namespace UI.MainApp
{
    public class SearchPanel : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_InputField inputField;

        void OnEnable()
        {
            Reset();
        }

        public void OnSearch()
        {
            string searchText = inputField.text;
            if (!string.IsNullOrEmpty(searchText))
            {
                Debug.Log("Searching for: " + searchText);
            }
            Reset();
        }
        void Reset()
        {
            inputField.text = "";
            
        }
    }
}
