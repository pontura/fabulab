using System;
using UI;
using UI.MainApp;
using Unity.Multiplayer.Center.Common;
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
        [SerializeField] GameObject thirdPanel;
        [SerializeField] GameObject tools;
        public TMPro.TMP_Text field2;
        public override void OnShow()
        {            
            firstPanel.SetActive(true);
            tools.SetActive(true); 
            UIManager.Instance.ShowBack(false);
            secondButton.SetActive(false);
            thirdPanel.SetActive(false);
            field.text = "Genial! ahora sigamos con el resto del cuerpo";   
            field2.text = "Cuando termines apretá este botón";
            characterScrollContent.GetComponent<Animation>().Play("off");
            presetsUI.Toggle(); 
            presetsUI.tabs.Clicked(presetsUI.tabs.All[0]);
            Events.OnSaveCharacterDone += OnSaveCharacterDone;  
        }
        public void FirstButtonClicked()
        {
            firstPanel.SetActive(false);
            secondButton.SetActive(true);
            presetsUI.tabs.Clicked(presetsUI.tabs.All[1]);
            thirdPanel.SetActive(false);
            Invoke("HideSecondPanel", 2);
        }
        void HideSecondPanel()
        {
            SecondButtonClicked();
        }
        public void SecondButtonClicked()
        {
            firstPanel.SetActive(false);
            secondButton.SetActive(false);
            thirdPanel.SetActive(true);
        }
         public void ThirdButtonClicked()
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
