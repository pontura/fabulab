using System;
using UI;
using UI.MainApp;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoarding_AllBody : OnBoarding_GenericText
    {
        [SerializeField] CharacterEdition characterEdition;
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] GameObject characterScrollContent;
        [SerializeField] GameObject secondButton;
        [SerializeField] GameObject firstPanel;
        public override void OnShow()
        {            
            firstPanel.SetActive(true);
            secondButton.SetActive(false);
            field.text = "Genial! ahora sigamos con el resto del cuerpo";   
            characterScrollContent.GetComponent<Animation>().Play("off");
            presetsUI.Clicked(true);   
            Events.OnSaveCharacterDone += OnSaveCharacterDone;          
        }
        public void FirstButtonClicked()
        {
            firstPanel.SetActive(false);
            secondButton.SetActive(true);
        }
        public void SecondButtonClicked()
        {
            Done();
            // TO-DO descomentar para que lo guarde de verdad:
            //characterEdition.OnSaveClicked();
        }
        public void HidePanel()
        {
            gameObject.SetActive(false);
        }
        private void OnSaveCharacterDone()
        {
            Done();
        }

        public override void OnHide()
        {
            Events.OnSaveCharacterDone -= OnSaveCharacterDone;  
        }
    }
}
