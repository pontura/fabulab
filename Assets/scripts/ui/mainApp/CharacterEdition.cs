using BoardItems;
using BoardItems.Characters;
using UnityEngine;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;

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
                case UIManager.screenType.WorkDetail:
                    break;
                case UIManager.screenType.Creation_Character:
                    Events.ShowUndo(true);
                    SetChangesMade(false);
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
            if (StoryMakerEvents.isEditing)
                Done();
            else
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
            UIManager.Instance.hasUnsavedChanges = this.changesMade;
        }
        
        private void OnActivateUIButtons(bool isOn)
        {
            DoneBtn.SetActive(isOn);
        }
        public void Done()
        {
            string anim = Data.Instance.characterAnimsManager.defaultEdit.name;
            Events.OnCharacterAnim("", anim);
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
            savingPart = true;
            Events.EmptyCharacterItemsButExlude((CharacterPartsHelper.parts)(int)UIManager.Instance.zoomManager.lastZoom);
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
            string partID = UIManager.Instance.zoomManager.lastZoom.ToString();
            FirebaseStoryMakerDBManager.Instance.DeleteBodypartPreset(presetID, partID, OnPartDeleted);
        }
        void OnPartDeleted(string result)
        {
            Debug.Log("DeletePart: " + result);
            Cancel();
        }
        public void SetButtons()
        {
            Debug.Log("Data.Instance.charactersData.GetCurrent() SetButtons: " + Data.Instance.charactersData.GetCurrent());
            if (Data.Instance.userData.isAdmin)
            {
                savePartButton.SetActive(true);
                deletePartButton.SetActive(Data.Instance.charactersData.PresetID != "");

                savePartButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Save " + UIManager.Instance.zoomManager.lastZoom.ToString();
                deletePartButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Delete " + UIManager.Instance.zoomManager.lastZoom.ToString();
            } else
            {
                savePartButton.SetActive(false);
                deletePartButton.SetActive(false);
            }
            saveCharacterButton.SetActive(Data.Instance.charactersData.GetCurrent() != "");

            saveNewCharacterButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Guardar";
        }
        public void Save()
        {
            savingPart = false;
            Events.SetCharacterIdle("");
            savePanel.SetActive(false);
            Data.Instance.charactersData.SetCurrentID("");// Resetea si hay un character elegido.
            Invoke("SaveWork", 0.1f);
        }
        public void Replace()// Guarda la version editada del personaje.
        {
            savingPart = false;
            Events.SetCharacterIdle("");
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
            Events.OnBodyPartActive(CharacterPartsHelper.parts.HEAD);
            Events.Zoom(ZoomStates.HEAD, true);
            savePanel.SetActive(false);
            Invoke("SaveProfilePicDelayed", 1);
        }
        void SaveProfilePicDelayed()
        {
            UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.charactersData.thumbSize, SaveProfilePictureDone);
        }
        public void SaveProfilePictureDone(Texture2D tex)
        {
            string thumb = System.Convert.ToBase64String(tex.EncodeToPNG());
            FirebaseStoryMakerDBManager.Instance.SaveProfilePicture(thumb);
        }
        void SaveWork()
        {
            Events.OnNewBodyPartSelected(null);
            UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.charactersData.thumbSize, OnTakeShotDone);
        }
        bool savingPart = false;
        public void OnTakeShotDone(Texture2D tex)
        {
            if(savingPart)
                Data.Instance.charactersData.SavePartCharacter(tex, (CharacterPartsHelper.parts)(int)UIManager.Instance.zoomManager.currentZoom);
            else
                Data.Instance.charactersData.SaveCharacter(tex);
        }
    }

}