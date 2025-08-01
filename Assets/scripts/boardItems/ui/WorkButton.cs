using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BoardItems.UI
{
    public class WorkButton : MonoBehaviour
    {
        public bool isPakapaka;
        public Image image;
        public GameObject pakapakaLogo;
        public int PakaPakaObjectID; // esta en AlbumData  pakapakaAlbum

        public void Init(Sprite sprite, bool isPakapaka)
        {
            image.sprite = sprite;
            this.isPakapaka = isPakapaka;
            if (isPakapaka)
                pakapakaLogo.SetActive(true);
            else
                pakapakaLogo.SetActive(false);
        }
        public void ClickedPakaPaka()
        {
            if (isPakapaka)
                UIManager.Instance.albumUI.DuplicateWork(PakaPakaObjectID);
        }
    }

}