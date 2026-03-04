using BoardItems.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BoardItems.BoardData;
using static PalettesManager;

namespace UI
{
    public class PresetButton : MonoBehaviour
    {
        [SerializeField] Image image;
        public SOPartData workData;
        public colorNames color;
        public int partToColorizeID = 0; //0 = amrs; 1 = legs
        System.Action<PresetButton> OnClicked;

        public void Init(System.Action<PresetButton> OnClicked, SOPartData workData)
        {
            this.workData = workData;
            this.OnClicked = OnClicked;
            image.sprite = workData.GetSprite();
        }
        public void Init(System.Action<PresetButton> OnClicked, colorNames _color, int partToColorizeID)
        {
            this.partToColorizeID = partToColorizeID;

            if(partToColorizeID == 0)
                image.sprite = Data.Instance.palettesManager.arm;
            else if (partToColorizeID == 1)
                image.sprite = Data.Instance.palettesManager.leg;
             else
                image.sprite = Data.Instance.palettesManager.eyebrows;

            this.color = _color;
            this.OnClicked = OnClicked;
            List<Color> allColors = Data.Instance.palettesManager.GetColorsByName(_color);
            image.color = allColors[0];
        }
        public void Init(System.Action<PresetButton> OnClicked, colorNames _color)
        {
            this.color = _color;
            this.OnClicked = OnClicked;
            List<Color> allColors = Data.Instance.palettesManager.GetColorsByName(_color);
            image.color = allColors[0];
        }
        public void OnClick()
        {
            OnClicked(this);
        }
    }

}