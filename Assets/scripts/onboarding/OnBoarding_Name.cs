using UnityEngine;
using Yaguar.StoryMaker.DB;

namespace OnBoarding
{
    public class OnBoarding_Name : OnBoardingMain
    {
        [SerializeField] TMPro.TMP_Text field;
        [SerializeField] TMPro.TMP_InputField input;
         public override void OnShow()
        {
            field.text = "Hola,";                  
        }
        public void OnClicked()
        {
            string t = "Anónimo";
            if(input.text == "")
                Events.OnPopupTopSignalText("por ahora te llamarás ");
            else
                t = input.text;

            FirebaseStoryMakerDBManager.Instance.UpdateUsername(t);
            Done();              
        }
    }
}
