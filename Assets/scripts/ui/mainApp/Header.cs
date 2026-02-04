namespace UI.MainApp
{
    public class Header : MainScreen
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
        public void Create()
        {
            UIManager.Instance.Create();
        }
    }
}
