using UnityEngine;
using UnityEngine.UI;

namespace BoardItems.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public GameObject panel;
        public Button button;

        private void Start()
        {
            int id = 0;
            button.onClick.AddListener(() => Clicked(id));
        }
        public void ShowMainMenu(bool enable)
        {
            panel.SetActive(enable);
        }
        public void Clicked(int id)
        {
            UIManager.Instance.albumUI.ShowAlbum(true);
            ShowMainMenu(false);
        }
        
    }
}