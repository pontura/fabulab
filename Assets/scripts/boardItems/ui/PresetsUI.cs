using Common.UI;
using UnityEngine;

namespace BoardItems.UI
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
            Events.Zoom(id+1);
            isPreset = true;
            lastPartID = id;
            dragAndDropUI.gameObject.SetActive(false);
            presetsSelector.SetOn(true, id+1);
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
