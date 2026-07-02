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
                    field.text = "Vamos a crear tu primer personaje.";
                break;
                case OnBoardingManager.steps.characterDone:
                    field.text = "¡Muy bien! ¡Quedó increíble!";
                break;
                  case OnBoardingManager.steps.firstStoryIntro:
                    field.text = "Es hora de hacer una historia.";
                break;
                  case OnBoardingManager.steps.storyPresentation:
                    field.text = "¡Te damos la bienvenida a Fabulab!";
                break;
            }           
        }
       
    }
}
