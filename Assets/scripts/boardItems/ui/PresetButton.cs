using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static BoardItems.AlbumData;
using static PalettesManager;

namespace BoardItems.UI
{
    public class PresetButton : MonoBehaviour
    {
        [SerializeField] Image image;
        PresetsSelector manager;
        public CharacterData workData;
        public colorNames color;
        public int partToColorizeID = 0; //0 = amrs; 1 = legs

        public void Init(PresetsSelector manager, CharacterData workData)
        {
            this.workData = workData;
            this.manager = manager;
            image.sprite = workData.GetSprite();
        }
        public void Init(PresetsSelector manager, colorNames _color, int partToColorizeID)
        {
            this.partToColorizeID = partToColorizeID;

            if(partToColorizeID == 0)
                image.sprite = Data.Instance.palettesManager.arm;
            else
                image.sprite = Data.Instance.palettesManager.leg;

            this.color = _color;
            this.manager = manager;
            List<Color> allColors = Data.Instance.palettesManager.GetColorsByName(_color);
            image.color = allColors[0];
        }
        public void OnClick()
        {
            manager.OnClicked(this);
        }
    }

}