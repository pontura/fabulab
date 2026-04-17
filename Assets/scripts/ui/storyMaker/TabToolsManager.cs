using UnityEngine;

namespace UI.MainApp
{
    public class TabToolsManager : MonoBehaviour
    {
        [SerializeField] GameObject backgrounds;

        void Start()
        {
            backgrounds.SetActive(false);
        }
        public void SetOn(int tabID)
        {
            backgrounds.SetActive(false);
            switch (tabID)
            {
                case 0:
                    backgrounds.SetActive(true);
                    break;
            }
        }
    }
}
