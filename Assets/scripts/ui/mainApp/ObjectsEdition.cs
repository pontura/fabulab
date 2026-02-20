using UnityEngine;

namespace UI.MainApp
{
    public class ObjectsEdition : MainScreen
    {
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
                    changesMade = false;
                    SetButtons();
                    Show(true);
                    savePanel.SetActive(false);
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
            saveCharacterButton.SetActive(Data.Instance.albumData.GetCurrent() != "");
        }
        public void Save()
        {
            savePanel.SetActive(false);
            Data.Instance.albumData.SetCurrentID("");// Resetea si hay un character elegido.
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
            UIManager.Instance.zoomManager.Zoom(BoardItems.Characters.CharacterData.parts.HEAD, true);
            savePanel.SetActive(false);
            Invoke("SaveProfilePicDelayed", 1);
        }
        void SaveProfilePicDelayed()
        {
            UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.albumData.thumbSize, SaveProfilePicture);
        }
        public void SaveProfilePicture(Texture2D tex)
        {
            print("TO-DO: graba la profilepicture");
        }
        void SaveWork()
        {
            UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.albumData.thumbSize, OnTakeShotDone);
        }
        public void OnTakeShotDone(Texture2D tex)
        {
            Data.Instance.albumData.SaveCharacter(tex);
        }

    }

}