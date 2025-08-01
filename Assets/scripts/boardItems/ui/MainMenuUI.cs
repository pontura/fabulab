using UnityEngine;

namespace BoardItems.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public GameObject mainMenu;
        public GameObject moreInfo;

        private void Start()
        {
            //ShowMainMenu(true);
        }
        public void ShowMainMenu(bool enable)
        {
            mainMenu.SetActive(enable);
            if (enable)
                AudioManager.Instance.musicManager.Play("main");
        }

        public void SelectGallery()
        {
            //AudioManager.Instance.uiSfxManager.PlayUISfxTransp("open",-2);
            AudioManager.Instance.uiSfxManager.PlayTransp("click", -3);
            UIManager.Instance.gallerySelectorUI.ShowGallerySelector(true);
            mainMenu.SetActive(false);
        }

        public void OpenAlbum()
        {
            //AudioManager.Instance.uiSfxManager.PlayUISfx("open");
            AudioManager.Instance.uiSfxManager.PlayTransp("click", -3);
            AudioManager.Instance.musicManager.Play("album");
            UIManager.Instance.albumUI.ShowAlbum(true);
            mainMenu.SetActive(false);
        }

        public void New()
        {
            Data.Instance.albumData.ResetCurrentID();
            SelectGallery();
        }

        public void ShowMoreInfo(bool enable)
        {
            if (enable)
                AudioManager.Instance.uiSfxManager.PlayTransp("click", -3);
            else
                AudioManager.Instance.musicManager.Play("main");
            moreInfo.SetActive(enable);
        }
    }

}