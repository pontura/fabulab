using System;
using UI;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoarding_AllBody : OnBoarding_GenericText
    {
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] GameObject characterScrollContent;
        public override void OnShow()
        {            
            field.text = "Genial! ahora sigamos con el resto del cuerpo";   
            characterScrollContent.GetComponent<Animation>().Play("off");
            presetsUI.Clicked(true);   
            Events.OnSaveCharacterDone += OnSaveCharacterDone;          
        }
        public void HidePanel()
        {
            gameObject.SetActive(false);
        }
        private void OnSaveCharacterDone()
        {
            Invoke("Done", 1);
        }

        public override void OnHide()
        {
            Events.OnSaveCharacterDone -= OnSaveCharacterDone;  
        }
    }
}
