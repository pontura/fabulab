using System;
using UI;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoarding_ChooseHead : OnBoarding_GenericText
    {
        public GameObject button;
        [SerializeField] GameObject characterEdition;
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] GameObject PresetDragAndDropToggle;
        [SerializeField] GameObject characterScrollContent;
        public override void OnShow()
        {            
            button.gameObject.SetActive(false);
            field.text = "Elegí una cara.";     
            characterEdition.gameObject.SetActive(true);
            presetsUI.Init();
            characterScrollContent.GetComponent<Animation>().Play("on");
            PresetDragAndDropToggle.gameObject.SetActive(false);
            Events.SetChangesMade += SetChangesMade;
        }

        private void SetChangesMade(bool obj)
        {
            button.gameObject.SetActive(true);
        }

        public override void OnHide()
        {
            Events.SetChangesMade -= SetChangesMade;
           // characterScrollContent.GetComponent<Animation>().Play("off");
        }
         public override  void ShowPanelsBack() {}
    }
}
