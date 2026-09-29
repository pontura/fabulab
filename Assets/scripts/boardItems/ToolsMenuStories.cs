using UnityEngine;

namespace BoardItems
{
    public class ToolsMenuStories : MonoBehaviour
    {
        public void Show(bool isOn) {
            gameObject.SetActive(isOn);
        }
    }
}