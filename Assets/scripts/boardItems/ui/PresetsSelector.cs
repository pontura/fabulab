using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;
using static BoardItems.AlbumData;

namespace BoardItems.UI
    {
        public class PresetsSelector : MonoBehaviour
        {
            [SerializeField] Transform container;
            [SerializeField] PresetButton itemButton;
            [SerializeField] Dictionary<ItemData, ItemButton> all;

            private void Awake()
            {
                Reset();
            }
            public void SetOn(bool isOn, int partID)
            {
                int artID = 0;
                gameObject.SetActive(isOn);
                if (isOn)
                {

                    Utils.RemoveAllChildsIn(container);
                    CharacterAnims.anims anim = CharacterAnims.anims.edit;
                    Events.OnCharacterAnim(0, anim);

                    List<WorkData> all = Data.Instance.albumData.GetPreset(partID);
                    artID++;
                    foreach (WorkData wd in all)
                    {
                        if (wd.thumb != null)
                        {
                            PresetButton b = Instantiate(itemButton, container);
                            b.Init(this, wd);
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
                UIManager.Instance.boardUI.LoadPreset(pb.workData);
            }
        }
    }
