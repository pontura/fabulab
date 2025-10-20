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
            AudioManager.Instance.uiSfxManager.Play("click");
            Data.Instance.galeriasData.SetGallery(id);
            Events.InitGallery(Data.Instance.galeriasData.gallery, true);

            gallerySelector.SetActive(false);
        }
        public void SelectGallery(int id, bool editMode)
        {
            //AudioManager.Instance.uiSfxManager.PlayNextScaleUISfx("click");
            AudioManager.Instance.uiSfxManager.Play("click");
            Data.Instance.galeriasData.SetGallery(id);
            Events.InitGallery(Data.Instance.galeriasData.gallery, editMode);
            //RectTransform vp = inventarioScroll.viewport;
            //Destroy(vp.transform.GetChild(0).gameObject);
            //GameObject gal = GameObject.Instantiate(Data.Instance.galeriasData.galleries[id], vp.transform);
            //gal.transform.localScale = Vector3.one;
            //gal.transform.localPosition = new Vector3(0f, 255f, 0f);
            //inventarioScroll.content = gal.transform as RectTransform;

            gallerySelector.SetActive(false);
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