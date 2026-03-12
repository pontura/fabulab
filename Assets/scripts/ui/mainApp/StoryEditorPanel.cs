using System.Diagnostics;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp
{
    public class StoryEditorPanel : MainScreen
    {

        UIManager.screenType last;
        protected override void ShowScreen(UIManager.screenType type) 
        {
            switch(type)
            {
                case UIManager.screenType.StoryMaker:
                    Events.Zoom(ZoomStates.STORY, false);
                    Show(true);
                    StoryMakerEvents.EnableInputManager(true);
                    break;
                default:
                    if (last == UIManager.screenType.StoryMaker) {
                        StoryMakerEvents.Restart();
                        Show(false);
                        StoryMakerEvents.EnableInputManager(false);
                    }
                    break;
            }
            last = type;
        }        
    }
}