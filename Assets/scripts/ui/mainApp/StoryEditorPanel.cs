using BoardItems.Characters;
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
                    StoryMakerEvents.Restart();
                    Show(true);
                    StoryMakerEvents.EnableInputManager(true);
                    Events.ColorizeBG(PalettesManager.colorNames.BG_CELESTE);
                    UIManager.Instance.boardUI.characterManager.gameObject.SetActive(false);
                    Invoke("Delayed", 0.5f);
                    break;
                default:
                    if (last == UIManager.screenType.StoryMaker) {
                        StoryMakerEvents.ClearScene();
                        Show(false);
                        StoryMakerEvents.EnableInputManager(false);
                    }
                    break;
            }
            last = type;
        }
        void Delayed()
        {
            StoryMakerEvents.SetEditing(false);
        }
    }
}