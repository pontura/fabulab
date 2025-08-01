using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace BoardItems.UI
{
    public class AlbumUI : MonoBehaviour
    {
        public GameObject album;
        public GameObject workBtn_prefab;
        public Transform worksContainer;
        public GameObject[] hardcodedObject;

        public void ShowAlbum(bool enable)
        {
            album.SetActive(enable);
            if (enable)
                Init();
            else
            {
                //AudioManager.Instance.uiSfxManager.PlayUISfx("back");
                AudioManager.Instance.uiSfxManager.PlayTransp("click", -5);
                UIManager.Instance.mainMenuUI.ShowMainMenu(true);
            }
        }

        void Init()
        {
            foreach (GameObject go in hardcodedObject)
                go.transform.SetParent(transform);

            foreach (Transform child in worksContainer)
            {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }

            for (int i = 0; i < Data.Instance.albumData.album.Count; i++)
            {
                AlbumData.WorkData wd = Data.Instance.albumData.album[i];
                if (wd.thumb != null)
                {
                    if (Data.Instance.galeriasData.ExistGallery(wd.galleryID))
                    {
                        GameObject go = Instantiate(workBtn_prefab, worksContainer);
                        Sprite sprite = Sprite.Create(wd.thumb, new Rect(0, 0, wd.thumb.width, wd.thumb.height), Vector2.zero);
                        Image[] imgs = go.GetComponentsInChildren<Image>();
                        imgs[0].color = Data.Instance.palettesManager.GetColor(Data.Instance.galeriasData.GetGalleryColorUI(wd.galleryID));
                        imgs[1].sprite = sprite;
                        go.GetComponent<Button>().onClick.AddListener(() => OpenWork(wd.id, sprite, imgs[0].color, wd.pkpkShared));
                    }
                }
            }
            foreach (GameObject go in hardcodedObject)
                go.transform.SetParent(worksContainer);

            AudioManager.Instance.uiSfxManager.PlayTransp("forward", 2);
        }
        public void DuplicateWork(int PakaPakaObjectID)
        {
            UIManager.Instance.boardUI.LoadPakapakaWork(PakaPakaObjectID);
            album.SetActive(false);
        }

        public void OpenWork(string id, Sprite sprite, Color colorUI, bool pkpkShared)
        {
            //AudioManager.Instance.uiSfxManager.PlayNextScaleUISfx("click");
            AudioManager.Instance.uiSfxManager.PlayTransp("click", 2);
            UIManager.Instance.workDetailUI.ShowWorkDetail(id, sprite, colorUI, pkpkShared, false);
            album.SetActive(false);
        }

        public void New()
        {
            UIManager.Instance.mainMenuUI.New();
            album.SetActive(false);
        }
    }

}