namespace UI.MainApp
{
    public class StoryEditorPanel : MainScreen
    {
        protected override void ShowScreen(UIManager.screenType type) 
        {
            switch(type)
            {
                case UIManager.screenType.StoryMaker:
                    Show(true);
                    break;
                default:
                    Show(false);
                    break;
            }
        }        
    }
}