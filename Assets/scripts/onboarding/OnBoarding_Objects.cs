using System.Collections;
using UI;
using UI.MainApp;
using UI.MainApp.Home.User;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace OnBoarding
{
    public class OnBoarding_Objects : OnBoarding_GenericText
    {
        public TMPro.TMP_Text field2;
        [SerializeField] GameObject part1;
        [SerializeField] GameObject part2;

        [SerializeField] StoryEditorScreen storyEditorScreen;
        [SerializeField] ObjectSelectionScreen objectSelectionScreen;

        public override void OnShow()
        {            
            field.text = "¿Qué objeto te gusta para tu historia?";
            field2.text = "Podés ponerlo donde más te guste";

            part1.gameObject.SetActive(true);
            part2.gameObject.SetActive(false);          

            Events.OnBoardingStepDone += OnBoardingStepDone;

            
            storyEditorScreen.OnTabClicked(0);
            objectSelectionScreen.Clicked(1);
        }

        private void OnBoardingStepDone(OnBoardingManager.steps steps)
        {
            if(active && steps != step) return;
            Step2();
            Events.OnBoardingStepDone -= OnBoardingStepDone;
        }

        void OnDestroy()
        {            
            Events.OnBoardingStepDone -= OnBoardingStepDone;
        }
        IEnumerator AddCharacterToScene()
        {
            StoryMakerEvents.SetEditing(true);
            yield return new WaitForSeconds(0.25f);
            storyEditorScreen.OnTabClicked(2);

            Data.Instance.charactersData.ResetCurrentID();
            StoryMakerEvents.SetEditing(true);  

            yield return new WaitForSeconds(0.25f);
            
            objectSelectionScreen.Clicked(1);
        }
        void Step2()
        {            
            part1.gameObject.SetActive(false);
            part2.gameObject.SetActive(true);
        }
        public override void ShowPanelsBack() {}
        public override void OnHide()
        {
            storyEditorScreen.OnTabClicked(4);
        }
    }
}
