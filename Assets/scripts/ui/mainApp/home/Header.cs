using UnityEngine;

namespace UI.MainApp
{
    public class Header : MonoBehaviour
    {
        [SerializeField] GameObject editBtn;

        public void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Home:
                    editBtn.SetActive(true);
                    break;
                case UIManager.screenType.UserScreen:
                    editBtn.SetActive(false);
                    break;
            }
        }
        public void Edit()
        {
            Events.ShowScreen(UIManager.screenType.UserScreen);
        }
        public void Create()
        {
            UIManager.Instance.Create();
        }
    }
}
