using BoardItems.Characters;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp
{
    public class ObjectsEdition : MainScreen
    {
        bool MyObject;
        [SerializeField] SceneObjectsPanel sceneObjectsPanel;
        [SerializeField] DragAndDropUI dragAndDropUI;
        [SerializeField] GameObject DoneBtn;
        [SerializeField] bool changesMade;

        void OnToggle(bool isOn)
        {
            UIManager.Instance.boardUI.snap = isOn;
        }
        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.WorkDetail:
                    break;
                case UIManager.screenType.Creation_Objects:
                    sceneObjectsPanel.Init();                   
                    Events.OnBodyPartActive(CharacterPartsHelper.parts.BODY);
                    switch (Data.Instance.sObjectsData.Type)
                    {
                        case BoardItems.BoardData.SObjectData.types.generic:
                            Events.Zoom(ZoomStates.NONE, false); 
                            Events.ShowUndo(true);
                            break;
                        case BoardItems.BoardData.SObjectData.types.background:
                            Events.Zoom(ZoomStates.BACKGROUND, false);
                            Events.ShowUndo(false);
                            break;
                    }
                    UIManager.Instance.boardUI.activeBoardItem.Init();
                    SetChangesMade(false);
                    changesMade = false;
                    Show(true);
                  //  savePanel.SetActive(false);
                    dragAndDropUI.SetOn(true);
                    dragAndDropUI.Init();
                    //if (StoryMakerEvents.isEditing)  Done(); // por si llegas desde la historia te manda al Save directamente.
                    break;
                default:
                    Show(false);
                    break;
            }
        }
        private void Start()
        {
          //  Events.ActivateUIButtons += OnActivateUIButtons;
            Events.SetChangesMade += SetChangesMade;
        }
        public override void OnDestroyed() 
        {
          //  Events.ActivateUIButtons -= OnActivateUIButtons;
            Events.SetChangesMade -= SetChangesMade;
        }   
        void SetChangesMade(bool _changesMade)
        {
            this.changesMade = _changesMade;
            UIManager.Instance.hasUnsavedChanges = this.changesMade;
        }
        
        //private void OnActivateUIButtons(bool isOn)
        //{
        //    DoneBtn.SetActive(isOn);
        //}
        public void Done()
        {
            SaveWork();
            UIManager.Instance.boardUI.toolsMenu.SetOff();
        }       
        
        void SaveWork()
        {
            Events.OnNewBodyPartSelected(null);
            UIManager.Instance.boardUI.screenshot.TakeShot(Vector2Int.zero, OnTakeShotDone);
        }
        public void OnTakeShotDone(Texture2D tex)
        {
            UIManager.Instance.workDetailUI.SetTexture(tex);
            Data.Instance.sObjectsData.SaveSO(tex);
        }
       
    }

}