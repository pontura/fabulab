using BoardItems;
using System.Collections.Generic;
using UnityEngine;
using BoardItems.BoardData;
using static PalettesManager;

namespace UI
{
    public class SOSelector : MonoBehaviour
    {
        [SerializeField] Transform container;
        [SerializeField] PresetButton itemButton;
        [SerializeField] Dictionary<ItemData, ItemButton> all;
        private void Awake()
        {
            Reset();
        }
        public void SetOn(bool isOn)
        {
            int artID = 0;
            gameObject.SetActive(isOn);
            if (isOn)
            {
                Utils.RemoveAllChildsIn(container);
               
                List<SOPartData> all = Data.Instance.charactersData.GetPreset(1);
                artID++;
                foreach (SOPartData wd in all)
                {
                    if (wd.thumb != null)
                    {
                        PresetButton b = Instantiate(itemButton, container);
                        b.Init(OnClicked, wd);
                    }
                }
            }
        }       
        public void Reset()
        {
            all = new Dictionary<ItemData, ItemButton>();
            Utils.RemoveAllChildsIn(container);
        }
        public void OnClicked(PresetButton pb)
        {
            OpenColors();
        }
        void OpenColors()
        {
            foreach (colorNames s in Data.Instance.palettesManager.backgrounds)
            {
                PresetButton b = Instantiate(itemButton, container);
                b.Init(OnClicked, s, 0);
            }
        }
    }
}
