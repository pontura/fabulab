using BoardItems;
using System.Collections.Generic;
using UnityEngine;
using static BoardItems.AlbumData;
using static PalettesManager;

namespace UI
{
    public class PresetsSelector : MonoBehaviour
    {
        [SerializeField] Transform container;
        [SerializeField] PresetButton itemButton;
        [SerializeField] Dictionary<ItemData, ItemButton> all;
        int partID;
        private void Awake()
        {
            Reset();
        }
        public void SetOn(bool isOn, int partID)
        {
            this.partID = partID;
            int artID = 0;
            gameObject.SetActive(isOn);
            if (isOn)
            {
                Utils.RemoveAllChildsIn(container);
                CharacterAnims.anims anim = CharacterAnims.anims.edit;
                Events.OnCharacterAnim(0, anim);
                print("set on " + partID);
                if (partID == 9) 
                    OpenColors();
                else
                {
                    List<CharacterData> all = Data.Instance.albumData.GetPreset(partID);
                    artID++;
                    foreach (CharacterData wd in all)
                    {
                        if (wd.thumb != null)
                        {
                            PresetButton b = Instantiate(itemButton, container);
                            b.Init(this, wd);
                        }
                    }
                }
            }
        }
        void OpenColors()
        {
            foreach (colorNames s in Data.Instance.palettesManager.arms)
            {
                PresetButton b = Instantiate(itemButton, container);
                b.Init(this, s, 0);
            }
            foreach (colorNames s in Data.Instance.palettesManager.legs)
            {
                PresetButton b = Instantiate(itemButton, container);
                b.Init(this, s, 1);
            }
            foreach (colorNames s in Data.Instance.palettesManager.eyebrow)
            {
                PresetButton b = Instantiate(itemButton, container);
                b.Init(this, s, 2);
            }
        }
        public void Reset()
        {
            all = new Dictionary<ItemData, ItemButton>();
            Utils.RemoveAllChildsIn(container);
        }
        public void OnClicked(PresetButton pb)
        {
            Events.SetChangesMade(true);
            Events.ActivateUIButtons(true);
            print("OnClicked " + partID);
            if (partID == 9) //arms and legs colors:
            {
                if(pb.partToColorizeID == 0)
                    Events.ColorizeArms(pb.color);
                else if (pb.partToColorizeID == 1)
                    Events.ColorizeLegs(pb.color);
                  else
                    Events.ColorizeEyebrows(pb.color);
            }
            else
            {
                Events.OnPresetLoaded(pb.workData.id);
                UIManager.Instance.boardUI.LoadPreset(pb.workData);
            }
        }
    }
}
