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
    //    public GameObject[] hardcodedObject;

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
        int artID = 0;
        void Init()
        {
            artID = 0;

            foreach (Transform child in worksContainer)
            {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }

            LoadNext();
        }
        
        void LoadNext()
        {
            print("LoadNext " + artID);
            if (artID >= Data.Instance.albumData.characters.Count) return;
            AlbumData.CharacterData wd = Data.Instance.albumData.characters[artID];
            artID++;
            if (wd.thumb != null)
            {
                //if (Data.Instance.galeriasData.ExistGallery(wd.galleryID))
                //{
                    GameObject go = Instantiate(workBtn_prefab, worksContainer);
                    print("go " + go);
                    RawImage rm = go.GetComponentInChildren<RawImage>();
                    UIManager.Instance.boardUI.GenerateThumb(wd, rm, LoadNext);
                    go.GetComponent<Button>().onClick.AddListener(() => OpenWork(wd.id));
                //}
            }
        }
        public void DuplicateWork(int PakaPakaObjectID)
        {
          //  UIManager.Instance.boardUI.LoadPakapakaWork(PakaPakaObjectID);
            album.SetActive(false);
        }

        public void OpenWork(string id)
        {
            //AudioManager.Instance.uiSfxManager.PlayNextScaleUISfx("click");
            // AudioManager.Instance.uiSfxManager.PlayTransp("click", 2);
            // UIManager.Instance.workDetailUI.ShowWorkDetail(id, sprite, colorUI, pkpkShared, false);
            UIManager.Instance.boardUI.LoadWork(id);
            album.SetActive(false);
        }

        public void New()
        {
            Data.Instance.albumData.ResetCurrentID();
            UIManager.Instance.gallerySelectorUI.ShowGallerySelector(true);
            album.SetActive(false);
        }
    }

}