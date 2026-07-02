using BoardItems;
using UI;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoarding_HeadModify : OnBoarding_GenericText
    {
        [SerializeField] GameObject characterEdition;
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] GameObject PresetDragAndDropToggle;
        [SerializeField] GameObject characterScrollContent;
        [SerializeField] Animation dragAndDropContainerAnim;
        public override void OnShow()
        {            
            field.text = "Agregá alguna pieza.";     
            characterEdition.gameObject.SetActive(true);
            presetsUI.DragAndDrop();
            characterScrollContent.GetComponent<Animation>().Play("on");
            PresetDragAndDropToggle.gameObject.SetActive(false);
            Events.OnStopDrag += OnStopDrag;
            dragAndDropContainerAnim.Play("on");
        }

        private void OnStopDrag(ItemInScene scene, Vector3 vector)
        {
            if(!active) return;
            Events.OnStopDrag -= OnStopDrag;
            Done();
        }
        public override void ShowPanelsBack() {}
        public override void OnHide()
        {
            characterScrollContent.GetComponent<Animation>().Play("off");
        }
    }
}
