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
        [SerializeField] GameObject[] togglesGO;
        [SerializeField] GameObject presetsDragAndDropToggleGO;
        bool isPreset;
        int lastPartID;
       
        public void SetOff()
        {
            gameObject.SetActive(false);
        }
        int characterEditorID = 0;
        public void Init()
        {
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
            dragAndDropUI.gameObject.SetActive(false);
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
                CharacterData.parts part = (CharacterData.parts)id;
                Events.Zoom(part, false);
                if (isPreset)
                    presetsSelector.SetOn(true, id);
                else
                    DragAndDrop();
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
                OnTabClicked(lastPartID-1);
            else
                DragAndDrop();
        }
        void SetToggle()
        {
            togglesGO[0].SetActive(false);
            togglesGO[1].SetActive(false);

            if (isPreset)
                togglesGO[0].SetActive(true);
            else
                togglesGO[1].SetActive(true);    
        }
        
    }
}
