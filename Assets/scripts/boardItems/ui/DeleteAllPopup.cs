using UnityEngine;

namespace BoardItems.UI
{
    public class DeleteAllPopup : MonoBehaviour
    {
        public GameObject panel;

        void Start()
        {
            panel.SetActive(false);
        }

        public void Init()
        {
            panel.SetActive(true);
        }
        public void Yes()
        {
            GetComponent<BoardUI>().ResetBoardConfirmed();
            panel.SetActive(false);
        }
        public void No()
        {
            panel.SetActive(false);
        }
    }

}