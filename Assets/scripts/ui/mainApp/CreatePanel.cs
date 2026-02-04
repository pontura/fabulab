namespace UI.MainApp
{
    public class CreatePanel : MainScreen
    {
        protected override void ShowScreen(UIManager.screenType type) 
        {
            switch(type)
            {
                case UIManager.screenType.Create:
                    Show(true);
                    break;
                default:
                    Show(false);
                    break;
            }
        }
        public void Create(int id)
        {
            UIManager.Instance.CreateSelected(id);
        }
    }
}