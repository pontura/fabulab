using System;
using BoardItems;
using UI;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoarding_Move : OnBoarding_GenericText
    {
        [SerializeField] GameObject characterEdition;
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] GameObject PresetDragAndDropToggle;
        [SerializeField] GameObject characterScrollContent;
        [SerializeField] Animation dragAndDropContainerAnim;
        public override void OnShow()
        {            
            field.text = "Mové y organizá las piezas a tu gusto";     
            characterEdition.gameObject.SetActive(true);
            presetsUI.DragAndDrop();
            characterScrollContent.GetComponent<Animation>().Play("on");
            PresetDragAndDropToggle.gameObject.SetActive(false);
        }
         public override void OnHide()
        {
            dragAndDropContainerAnim.Play("off");
        }
    }
}
