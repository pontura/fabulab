using System;
using BoardItems;
using UI;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoarding_Move : OnBoarding_GenericText
    {
        [SerializeField] GameObject masker;
        [SerializeField] GameObject characterEdition;
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] GameObject PresetDragAndDropToggle;
        [SerializeField] GameObject characterScrollContent;
        [SerializeField] Animation dragAndDropContainerAnim;
        public override void OnShow()
        {            
            masker.SetActive(false);
            field.text = "Arrastrá y organizá las piezas a tu gusto";
            characterEdition.gameObject.SetActive(true);
            presetsUI.DragAndDrop();
            characterScrollContent.GetComponent<Animation>().Play("on");
            PresetDragAndDropToggle.gameObject.SetActive(false);
            Events.OnStopDrag += OnStopDrag;
           //Events.OnStartDrag += OnStartDrag;
        }

        private void OnStartDrag(ItemInScene scene, Vector3 vector)
        {
            masker.SetActive(true);
           // Events.OnStartDrag -= OnStartDrag;
        }

        private void OnStopDrag(ItemInScene scene, Vector3 vector) {
            masker.SetActive(false);
            field.text = "Tocá las piezas en el dibujo para modificarlas";
        }
        public override void OnHide()
        {
            Events.OnStopDrag -= OnStopDrag;
            dragAndDropContainerAnim.Play("off");
        }
    }
}
