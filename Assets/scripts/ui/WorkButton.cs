using UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
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
        
    }

}