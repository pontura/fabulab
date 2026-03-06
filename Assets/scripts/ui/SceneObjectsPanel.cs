using BoardItems.BoardData;
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
        [SerializeField] ToggleButton groupToggle;

        void OnToggle(bool isOn)
        {
            UIManager.Instance.boardUI.snap = isOn;
        }
        void OnToggleGroup(bool isOn)
        {
            Events.SetGroupToolsOn(isOn);
        }
        public void SetOff()
        {
            snapToggle.Show(false);
            groupToggle.Show(false);
            gameObject.SetActive(false);
        }
        string characterEditorID = "";
        public void Init()
        {
            snapToggle.Init(OnToggle, UIManager.Instance.boardUI.snap);
            groupToggle.Init(OnToggleGroup, false);

            dragAndDropUI.transform.SetParent(dragAndDropContainer);
            dragAndDropUI.Init();

            gameObject.SetActive(true);

            CharacterAnims.anims anim = CharacterAnims.anims.edit;
            Events.EditMode(true);
            Events.OnCharacterAnim(characterEditorID, anim);
            tabs.Init(OnTabClicked, 0);

            if(Data.Instance.sObjectsData.Type == SObjectData.types.background)
            {
                tabs.ShowTab(new System.Collections.Generic.List<bool> { true, true, true });
            }
            else
            {
                tabs.ShowTab(new System.Collections.Generic.List<bool> { true, true, false });
            }

        }
        void OnTabClicked(int id)
        {
            dragAndDropContainer.gameObject.SetActive(false);
            if (id == 0)
                DragAndDrop();
            else
            {
                soSelector.SetOn(true);
                if (id == 1)
                {
                    soSelector.SetColores();
                }
                if (id == 2)
                {
                    soSelector.SetObjects();
                }
            }
        }
        public void DragAndDrop()
        {
            soSelector.gameObject.SetActive(false);
            dragAndDropContainer.gameObject.SetActive(true);
            dragAndDropUI.SetOn(true);
        }

    }
}
