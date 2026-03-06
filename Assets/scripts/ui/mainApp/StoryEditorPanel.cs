using System.Diagnostics;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp
{
    public class StoryEditorPanel : MainScreen
    {
        protected override void ShowScreen(UIManager.screenType type) 
        {
            switch(type)
            {
                case UIManager.screenType.StoryMaker:
                    Events.Zoom(0, false);
                    Show(true);
                    StoryMakerEvents.EnableInputManager(true);
                    break;
                default:
                    StoryMakerEvents.Restart();
                    Show(false);                    
                    StoryMakerEvents.EnableInputManager(false);
                    break;
            }
        }        
    }
}