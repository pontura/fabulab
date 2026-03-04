using Common.UI;
using UnityEngine;

namespace UI
{
    public class SceneObjectsPanel : MonoBehaviour
    {
        [SerializeField] SOSelector soSelector;
        [SerializeField] Transform dragAndDropContainer;
        [SerializeField] DragAndDropUI dragAndDropUI;
        [SerializeField] TabController tabs;
        [SerializeField] ToggleButton snapToggle;

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
            dragAndDropUI.Init();
            gameObject.SetActive(true);
            CharacterAnims.anims anim = CharacterAnims.anims.edit;
            Events.EditMode(true);
            Events.OnCharacterAnim(characterEditorID, anim);
            tabs.Init(OnTabClicked, 0);
        }
        void OnTabClicked(int id)
        {
            dragAndDropContainer.gameObject.SetActive(false);
            if (id ==0)
                DragAndDrop();
            else
                soSelector.SetOn(true);
        }
        public void DragAndDrop()
        {
            soSelector.gameObject.SetActive(false);
            dragAndDropContainer.gameObject.SetActive(true);
            dragAndDropUI.SetOn(true);
        }

    }
}
