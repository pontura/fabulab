using BoardItems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp
{
    public class GallerySelectorUI : MainScreen
    {
        public ScrollRect inventarioScroll;

        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Galleries:
                    Show(true);
                    break;
                default:
                    Show(false);
                    break;
            }
        }

        public override void OnInit()
        {
            Events.ResetItems();
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
                Events.OnNewCharacter();
                GaleriasData.GalleryData gd = Data.Instance.galeriasData.GetGallery(id);
                UIManager.Instance.InitGallery(gd, true, null);
            }
            AudioManager.Instance.uiSfxManager.Play("click");
        }
        List<int> allGalleriesToAdd;
        int galleryNum = 0;
        public void SelectGallery(List<int> allGalleriesToAdd)
        {
            this.allGalleriesToAdd = allGalleriesToAdd;
            galleryNum = 0;
            LoadNextGallery();
        }
        void LoadNextGallery()
        {
            if (galleryNum >= allGalleriesToAdd.Count)
            {
                Events.GalleryDone();
            } else
            {
                GaleriasData.GalleryData gd = Data.Instance.galeriasData.GetGallery(allGalleriesToAdd[galleryNum]);
                UIManager.Instance.InitGallery(gd, true, LoadNextGallery);
                galleryNum++;
            }
        }

        public void BackToMainMenu()
        {
            UIManager.Instance.Home();
        }

    }

}