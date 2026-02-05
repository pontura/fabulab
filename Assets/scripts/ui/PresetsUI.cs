using BoardItems.Characters;
using Common.UI;
using UnityEngine;

namespace UI
{
    public class PresetsUI : MonoBehaviour
    {
        [SerializeField] PresetsSelector presetsSelector;
        [SerializeField] DragAndDropUI dragAndDropUI;
        [SerializeField] TabController tabs;
        [SerializeField] TMPro.TMP_Text toggleField;
        bool isPreset;
        int lastPartID;
       
        public void SetOff()
        {
            gameObject.SetActive(false);
        }
        int characterEditorID = 0;
        public void Init()
        {
            gameObject.SetActive(true);
            CharacterAnims.anims anim = CharacterAnims.anims.edit;
            Events.EditMode(true);
            Events.OnCharacterAnim(characterEditorID, anim);
            SetToggle();
        }
        void OnEnable()
        {
            Invoke("Delayed", 0.1f);
        }
        void Delayed()
        {
            tabs.Init(OnTabClicked);
        }
        void OnTabClicked(int id)
        {
            Events.OnPresetReset(); // Resetea si hay un preset abierto
            print(id);
            isPreset = true;
            id += 1;
            lastPartID = id;
            dragAndDropUI.gameObject.SetActive(false);

            if (id > 4) id += 2; // porque las manos y los pies ocupan 2 ids:
            if(id == 9) // arms and legs
            {
                Events.Zoom(0);
                presetsSelector.SetOn(true, 9);
            }
            else
            {
                CharacterData.parts part = (CharacterData.parts)id;
                Events.Zoom(part);
                presetsSelector.SetOn(true, id);
            }
                
        }
        public void DragAndDrop()
        {
            isPreset = false;
            presetsSelector.gameObject.SetActive(false);
            dragAndDropUI.SetOn(true);
        }
        public void Toggle()
        {
            isPreset = !isPreset;
            SetToggle();
            if (isPreset)
                OnTabClicked(lastPartID);
            else
                DragAndDrop();
        }
        void SetToggle()
        {
            toggleField.text = isPreset ? "Drag & Drop" : "Presets";           
        }
        
    }
}
