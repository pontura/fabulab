namespace UI.MainApp
{
    public class HomePage : MainScreen
    {
        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Home:
                    Show(true);
                    break;
                default:
                    Show(false);
                    break;
            }
        }
    }
}
