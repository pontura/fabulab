using UnityEngine;

namespace UI.MainApp
{
    public class PresetsPreviewUI : MainScreen
    {
        public bool isPreview;
        [SerializeField] GameObject savePanel;
        [SerializeField] TMPro.TMP_Text togglePreviewField;
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] PreviewUI previewUI;
        public GameObject DoneBtn;

        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Creation:
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
            SetTogglePreview();
        }
        public void TogglePreview()
        {
            isPreview = !isPreview;
            SetTogglePreview();
        }
        void SetTogglePreview()
        {
            togglePreviewField.text = isPreview ? "Edit" : "Preview";
            if (!isPreview)
            {
                presetsUI.Init();
                previewUI.SetOff();
            }
            else
            {
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
            UIManager.Instance.boardUI.SaveWork();
        }
        public void Save()
        {
            savePanel.SetActive(false);
            UIManager.Instance.boardUI.SaveWork();
        }
        public void Replace()
        {
            savePanel.SetActive(false);
        }
        public void Cancel()
        {
            DoneBtn.SetActive(true);
            savePanel.SetActive(false);
        }

    }

}