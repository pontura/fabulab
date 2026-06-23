using UnityEngine;

namespace OnBoarding
{
    public class OnBoarding_GenericText : OnBoardingMain
    {
        public TMPro.TMP_Text field;
        public override void OnShow()
        {
            switch(step)
            {
                case OnBoardingManager.steps.title_character:
                    field.text = "Cómo te gustaría verte?";
                break;
                case OnBoardingManager.steps.characterDone:
                    field.text = "Muy bien! Ya tenemos tu personaje";
                break;
            }           
        }
    }
}
