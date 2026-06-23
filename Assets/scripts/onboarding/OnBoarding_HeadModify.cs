using System;
using BoardItems;
using UI;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoarding_HeadModify : OnBoarding_GenericText
    {
        [SerializeField] GameObject characterEdition;
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] GameObject PresetDragAndDropToggle;
        [SerializeField] GameObject characterScrollContent;
        public override void OnShow()
        {            
            field.text = "Ahora a crear!";     
            characterEdition.gameObject.SetActive(true);
            presetsUI.DragAndDrop();
            characterScrollContent.GetComponent<Animation>().Play("on");
            PresetDragAndDropToggle.gameObject.SetActive(false);
            Events.OnStopDrag += OnStopDrag;
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
