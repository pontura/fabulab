using BoardItems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp
{
    public class WorkDetailUI : MainScreen
    {
        public GameObject savedSignal;
        public Image workImage;
        public List<Image> borders;
        public GameObject confirmation;
        public GameObject sendedSign;
        public GameObject sendingSign;
       // public SharePanel sharePanel;
        public Animation pkpkAnim;
        string id;

        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.WorkDetail:
                    Show(true);
                    break;
                default:
                    Show(false);
                    break;
            }
        }

        public void SetTexture(Texture2D tex) {
            workImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }

        public void ShowWorkDetail(string _id, bool isNew) {
            AudioManager.Instance.musicManager.Play("work");
            id = _id;
            confirmation.SetActive(false);
            savedSignal.SetActive(isNew);
            Events.OnLoading(false);
        }

        public void ShowWorkDetail(string _id, Sprite sprite, bool isNew)
        {
            AudioManager.Instance.musicManager.Play("work");
            id = _id;
            workImage.sprite = sprite;
            confirmation.SetActive(false);
            savedSignal.SetActive(isNew);
            Events.OnLoading(false);
        }
        public void ContinueEditing()
        {
            BodyPart bp = UIManager.Instance.boardUI.activeBoardItem.GetBodyPart(UIManager.Instance.part);
            Events.OnNewBodyPartSelected(bp);
            Show(false);
        }

    }
}