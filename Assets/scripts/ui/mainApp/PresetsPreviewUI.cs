using BoardItems;
using UnityEngine;

namespace UI.MainApp
{
    public class PresetsPreviewUI : MainScreen
    {
        public bool isPreview;
        [SerializeField] GameObject savePanel;
        [SerializeField] GameObject savePartButton;
        [SerializeField] GameObject saveNewCharacterButton;
        [SerializeField] GameObject saveCharacterButton;
        [SerializeField] GameObject[] togglePreviewParts;
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] PreviewUI previewUI;
        public GameObject DoneBtn;

        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Creation:
                    SetButtons();
                    Show(true);
                    savePanel.SetActive(false);
                    OnActivateUIButtons(false);
                    Invoke("Delayed", 0.1f);
                    break;
                default:
                    Show(false);
                    break;
            }
        }

        private void Start()
        {
            Events.ActivateUIButtons += OnActivateUIButtons;
        }
        public override void OnDestroyed() 
        {
            Events.ActivateUIButtons -= OnActivateUIButtons;
        }   
        void Delayed()
        {
            isPreview = false;
            SetTogglePreview();
        }
        public void TogglePreview()
        {
            isPreview = !isPreview;
            SetTogglePreview();
        }
        void SetTogglePreview()
        {
            if (!isPreview)
            {
                togglePreviewParts[0].SetActive(false);
                togglePreviewParts[1].SetActive(true);
                presetsUI.Init();
                previewUI.SetOff();
            }
            else
            {
                togglePreviewParts[0].SetActive(true);
                togglePreviewParts[1].SetActive(false);
                previewUI.Init();
                presetsUI.SetOff();
            }
        }
        private void OnActivateUIButtons(bool isOn)
        {
            DoneBtn.SetActive(isOn);
        }
        public void Done()
        {
            CharacterAnims.anims anim = CharacterAnims.anims.edit;
            Events.OnCharacterAnim(0, anim);
            savePanel.SetActive(true);
            //UIManager.Instance.boardUI.SaveWork();
            UIManager.Instance.boardUI.toolsMenu.SetOff();
            isPreview = true;
            SetTogglePreview();
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
        public void SetButtons()
        {
            savePartButton.SetActive(Data.Instance.userData.isAdmin);
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