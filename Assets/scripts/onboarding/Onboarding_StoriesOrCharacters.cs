using UI;
namespace OnBoarding
{
    public class Onboarding_StoriesOrCharacters : OnBoardingMain
    {
        public void Characters()
        {
            UIManager.Instance.HomeCharacters();
        }
        public void Stories()
        {            
            UIManager.Instance.Home();
        }
    }
}
