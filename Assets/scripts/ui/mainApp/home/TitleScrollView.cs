using UnityEngine;
namespace UI.MainApp
{
    public class TitleScrollView : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_Text titleField;

        public void Init(string t)
        {
            titleField.text = t;
        }
    }

}