using BoardItems.Characters;
using Common.UI;
using UnityEngine;

namespace UI
{
    public class PresetsUI : MonoBehaviour
    {
        [SerializeField] PresetsSelector presetsSelector;
        [SerializeField] Transform dragAndDropContainer;
        [SerializeField] DragAndDropUI dragAndDropUI;
        [SerializeField] TabController tabs;
        [SerializeField] GameObject[] togglesGO;
        [SerializeField] GameObject presetsDragAndDropToggleGO;
        [SerializeField] ToggleButton snapToggle;

        bool isPreset;
        int lastPartID;
        void OnToggle(bool isOn)
        {
            UIManager.Instance.boardUI.snap = isOn;
        }
        public void SetOff()
        {
            snapToggle.Show(false);
            gameObject.SetActive(false);
        }
        int characterEditorID = 0;
        public void Init()
        {
            snapToggle.Init(OnToggle, UIManager.Instance.boardUI.snap);
            dragAndDropUI.transform.SetParent(dragAndDropContainer);
            dragAndDropUI.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            dragAndDropUI.GetComponent<RectTransform>().anchorMax = Vector2.one;
            dragAndDropUI.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            dragAndDropUI.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            isPreset = true;
            gameObject.SetActive(true);
            CharacterAnims.anims anim = CharacterAnims.anims.edit;
            Events.EditMode(true);
            Events.OnCharacterAnim(characterEditorID, anim);
            tabs.Init(OnTabClicked, 0);
            SetToggle();
        }
        void OnTabClicked(int id)
        {
            Events.OnPresetReset(); // Resetea si hay un preset abierto
            print(" preset clicked:" + id);
            //isPreset = true; 
            SetToggle();
            id += 1;
            lastPartID = id;
            dragAndDropContainer.gameObject.SetActive(false);
            presetsDragAndDropToggleGO.SetActive(true);
            if (id > 4) id += 2; // porque las manos y los pies ocupan 2 ids:
            if(id == 9) // arms and legs
            {
                isPreset = true;
                presetsDragAndDropToggleGO.SetActive(false);
                Events.Zoom(0, false);
                presetsSelector.SetOn(true, 9);
            }
            else
            {
                CharacterPartsHelper.parts part = (CharacterPartsHelper.parts)id;
                Events.Zoom(part, false);
                if (isPreset)
                    presetsSelector.SetOn(true, id);
                else
                    DragAndDrop();
            }

            Events.OnCharacterPartAnim(characterEditorID, (CharacterPartsHelper.parts)id);
        }
        public void DragAndDrop()
        {
            isPreset = false;
            presetsSelector.gameObject.SetActive(false);
            dragAndDropContainer.gameObject.SetActive(true);
            dragAndDropUI.SetOn(true);
        }
        public void Toggle()
        {
            isPreset = !isPreset;

            SetToggle();

            if (isPreset)
                OnTabClicked(lastPartID-1);
            else
                DragAndDrop();
        }
        void SetToggle()
        {
            togglesGO[0].SetActive(false);
            togglesGO[1].SetActive(false);

            if (isPreset)
            {
                snapToggle.Show(false);
                togglesGO[0].SetActive(true);
            }
            else
            {
                snapToggle.Show(true);
                togglesGO[1].SetActive(true);
            } 
        }
        
    }
}
