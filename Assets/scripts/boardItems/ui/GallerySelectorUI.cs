using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace BoardItems.UI
{
    public class GallerySelectorUI : MonoBehaviour
    {
        public GameObject gallerySelector;
        public ScrollRect inventarioScroll;

        public void ShowGallerySelector(bool enable)
        {
            //UIManager.Instance.boardUI.ResetBoard();
            gallerySelector.SetActive(enable);
            Events.ResetItems();
            if (enable)
                AudioManager.Instance.musicManager.Play("gallery");
        }
        public void SelectGalleryClicked(int id)
        {
            if (id == 0)
            {
                List<int> allGalleries = new List<int>();
                allGalleries.Add(1);
                allGalleries.Add(2);
                SelectGallery(allGalleries);
            }
            else
            {
                GaleriasData.GalleryData gd = Data.Instance.galeriasData.GetGallery(id);
                Events.InitGallery(gd, true, null);
            }
            gallerySelector.SetActive(false);
            AudioManager.Instance.uiSfxManager.Play("click");
        }
        List<int> allGalleriesToAdd;
        int galleryNum = 0;
        public void SelectGallery(List<int> allGalleriesToAdd)
        {
            this.allGalleriesToAdd = allGalleriesToAdd;
            galleryNum = 0;
            LoadNextGallery();
            gallerySelector.SetActive(false);
            AudioManager.Instance.uiSfxManager.Play("click");
        }
        void LoadNextGallery()
        {
            if (galleryNum >= allGalleriesToAdd.Count)
            {
                Events.GalleryDone();
            } else
            {
                GaleriasData.GalleryData gd = Data.Instance.galeriasData.GetGallery(allGalleriesToAdd[galleryNum]);
                Events.InitGallery(gd, true, LoadNextGallery);
                galleryNum++;
            }
        }

        public void BackToMainMenu()
        {
            //AudioManager.Instance.uiSfxManager.PlayUISfx("back");
            //AudioManager.Instance.uiSfxManager.PlayPrevScaleUISfx("click");
            AudioManager.Instance.uiSfxManager.PlayTransp("click", -5);
            UIManager.Instance.mainMenuUI.ShowMainMenu(true);
            gallerySelector.SetActive(false);
        }

    }

}