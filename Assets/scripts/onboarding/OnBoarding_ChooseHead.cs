using UI;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoarding_ChooseHead : OnBoarding_GenericText
    {
        [SerializeField] GameObject characterEdition;
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] GameObject PresetDragAndDropToggle;
        [SerializeField] GameObject characterScrollContent;
        public override void OnShow()
        {            
            field.text = "Elegí una carita";     
            characterEdition.gameObject.SetActive(true);
            presetsUI.Init();
            characterScrollContent.GetComponent<Animation>().Play("on");
            PresetDragAndDropToggle.gameObject.SetActive(false);
        }
        public override void OnHide()
        {
            characterScrollContent.GetComponent<Animation>().Play("off");
        }
         public override  void ShowPanelsBack() {}
    }
}
