using UnityEngine;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;

namespace OnBoarding
{
    public class OnBoarding_Name : OnBoardingMain
    {
        [SerializeField] TMPro.TMP_Text field;
        [SerializeField] TMPro.TMP_InputField input;
        
         public override void OnShow()
        {
            StoryMakerEvents.EnableStoryEdition(true);
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
