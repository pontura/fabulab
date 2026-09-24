using UnityEngine;

namespace BoardItems
{
    public class ToolsMenuStories : MonoBehaviour
    {
        public GameObject arrow;
        public void Init(Vector3 pos) {
            gameObject.SetActive(true);
            arrow.transform.position = pos;
        }
        public void Close()
        {
            gameObject.SetActive(false);            
        }
    }
}