using BoardItems;
using UnityEngine;
using Yaguar.StoryMaker.DB;

namespace UI.MainApp
{
    public class CharacterEdition : MainScreen
    {
        public bool isPreview;
        [SerializeField] GameObject savePanel;
        [SerializeField] GameObject savePartButton;
        [SerializeField] GameObject deletePartButton;
        [SerializeField] GameObject saveNewCharacterButton;
        [SerializeField] GameObject saveCharacterButton;
        [SerializeField] GameObject[] togglePreviewParts;
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] PreviewUI previewUI;
        [SerializeField] GameObject DoneBtn;
        [SerializeField] bool changesMade;

        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Creation_Character:
                    changesMade = false;
                    SetButtons();
                    Show(true);
                    savePanel.SetActive(false);
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
            Events.SetChangesMade += SetChangesMade;
        }
        public override void OnDestroyed() 
        {
            Events.ActivateUIButtons -= OnActivateUIButtons;
            Events.SetChangesMade -= SetChangesMade;
        }   
        void Delayed()
        {
            bool isEditingCharacter = (Data.Instance.charactersData.GetCurrent() != "");
            isPreview = isEditingCharacter;
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
            CharacterAnims.anims anim = CharacterAnims.anims.edit;
            Events.OnCharacterAnim(0, anim);
            savePanel.SetActive(true);
            //UIManager.Instance.boardUI.SaveWork();
            UIManager.Instance.boardUI.toolsMenu.SetOff();
            isPreview = true;
            SetTogglePreview();
            OnActivateUIButtons(false);
            SetButtons();
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
        public void DeletePart()
        {
            string presetID = Data.Instance.charactersData.PresetID;
            string partID = UIManager.Instance.zoomManager.part.ToString();
            FirebaseStoryMakerDBManager.Instance.DeleteBodypartPreset(presetID, partID, OnPartDeleted);
        }
        void OnPartDeleted(string result)
        {
            Debug.Log("DeletePart: " + result);
            Cancel();
        }
        public void SetButtons()
        {
            if (Data.Instance.charactersData.PresetID != "" && Data.Instance.userData.isAdmin)
            {
                savePartButton.SetActive(true);
                deletePartButton.SetActive(true);

                savePartButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Save " + UIManager.Instance.zoomManager.lastZoom.ToString();
                deletePartButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Delete " + UIManager.Instance.zoomManager.lastZoom.ToString();
            } else
            {
                savePartButton.SetActive(false);
                deletePartButton.SetActive(false);
            }
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