using UnityEditor.Presets;
using UnityEngine;

namespace UI.MainApp
{
    public class ObjectsEdition : MainScreen
    {
        [SerializeField] Transform dragAndDropContainer;
        [SerializeField] DragAndDropUI dragAndDropUI;
        [SerializeField] GameObject savePanel;
        [SerializeField] GameObject saveNewCharacterButton;
        [SerializeField] GameObject saveCharacterButton;
        [SerializeField] GameObject DoneBtn;
        [SerializeField] bool changesMade;

        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Creation_Objects:
                    Events.Zoom(BoardItems.Characters.CharacterPartsHelper.parts.BODY, false);
                    changesMade = false;
                    SetButtons();
                    Show(true);
                    savePanel.SetActive(false);
                    dragAndDropUI.SetOn(true);
                    dragAndDropUI.transform.SetParent(dragAndDropContainer);
                    break;
                default:
                    Show(false);
                    break;
            }
        }
        private void Start()
        {
            Events.ActivateUIButtons += OnActivateUIButtons;
            Events.SetChangesMade += SetChangesMade;
        }
        public override void OnDestroyed() 
        {
            Events.ActivateUIButtons -= OnActivateUIButtons;
            Events.SetChangesMade -= SetChangesMade;
        }   
        void SetChangesMade(bool _changesMade)
        {
            this.changesMade = _changesMade;
        }
        public bool ChangesMade()
        {
            return changesMade;
        }
        private void OnActivateUIButtons(bool isOn)
        {
            DoneBtn.SetActive(isOn);
        }
        public void Done()
        {
            savePanel.SetActive(true);
            UIManager.Instance.boardUI.toolsMenu.SetOff();
            OnActivateUIButtons(false);
        }
        public void SavePart()
        {
            Events.EmptyCharacterItemsButExlude(UIManager.Instance.zoomManager.lastZoom);
            UIManager.Instance.zoomManager.ZoomToLastPart();
            savePanel.SetActive(false);
            Invoke("SavePartDelayed", 1);
        }
        void SavePartDelayed()
        {
            SaveWork();
        }
        public void SetButtons() { 
            saveCharacterButton.SetActive(Data.Instance.charactersData.GetCurrent() != "");
        }
        public void Save()
        {
            savePanel.SetActive(false);
            Data.Instance.charactersData.SetCurrentID("");// Resetea si hay un character elegido.
            SaveWork();
        }
        public void Replace()// Guarda la version editada del personaje.
        {
            savePanel.SetActive(false);
            SaveWork();
        }
        public void Cancel()
        {
            DoneBtn.SetActive(true);
            savePanel.SetActive(false);
        }
        public void SaveProfilePic()
        {
            print("SaveProfilePic");
            UIManager.Instance.zoomManager.Zoom(BoardItems.Characters.CharacterPartsHelper.parts.HEAD, true);
            savePanel.SetActive(false);
            Invoke("SaveProfilePicDelayed", 1);
        }
        void SaveProfilePicDelayed()
        {
            UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.charactersData.thumbSize, SaveProfilePicture);
        }
        public void SaveProfilePicture(Texture2D tex)
        {
            print("TO-DO: graba la profilepicture");
        }
        void SaveWork()
        {
            UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.charactersData.thumbSize, OnTakeShotDone);
        }
        public void OnTakeShotDone(Texture2D tex)
        {
            Data.Instance.charactersData.SaveCharacter(tex);
        }

    }

}