using BoardItems;
using BoardItems.BoardData;
using BoardItems.Characters;
using System;
using UnityEngine;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp
{
    public class CharacterEdition : MainScreen
    {
        [SerializeField] bool isMyAvatar;
        public bool isPreview;
        [SerializeField] GameObject pictureBtn;
        [SerializeField] GameObject bgColorsBtn;
        //[SerializeField] GameObject savePanel;
        //[SerializeField] GameObject savePartButton;
        //[SerializeField] GameObject deletePartButton;
        //[SerializeField] GameObject saveNewCharacterButton;
        //[SerializeField] GameObject saveCharacterButton;
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
                    SetChangesMade(false);
                  //  SetButtons();
                    Show(true);
                   // savePanel.SetActive(false);
                    Invoke("Delayed", 0.1f);
                    break;
                default:
                    Show(false);
                    break;
            }
        }

        private void Start()
        {
           // Events.ActivateUIButtons += OnActivateUIButtons;
            Events.SetChangesMade += SetChangesMade;
        }
        public override void OnDestroyed() 
        {
           // Events.ActivateUIButtons -= OnActivateUIButtons;
            Events.SetChangesMade -= SetChangesMade;
        }   
        void Delayed()
        {
            string chID = Data.Instance.charactersData.GetCurrent();
            isMyAvatar = Data.Instance.charactersData.IsMyCharacter(chID);
            bool isEditingCharacter = false;

            print("SetTogglePreview chID: " + chID + " isMyavatar: " + isMyAvatar);

            if (chID == "" || isMyAvatar)
                isEditingCharacter = true;

            isPreview = !isEditingCharacter;

            if (StoryMakerEvents.isEditing)
            {
                Events.ShowUndo(true);
                DoneBtn.SetActive(true);            
                Done();
            }
            else
            {
                SetTogglePreview();
                Events.ShowUndo(false);
            }

        }
        public void TogglePreview()
        {
            if (!isMyAvatar) DoneBtn.SetActive(true); // si venias viendo un character de otro te permite guardar una copia, pero si es tu avatar te muestra el boton de guardar desde el principio.
            isPreview = !isPreview;
            SetTogglePreview();
        }
        void SetTogglePreview()
        {
            print("SetTogglePreview isPreview: " + isPreview + " isMyAvatar: " + isMyAvatar);
            if (!isPreview)
            {
                pictureBtn.SetActive(false);
                bgColorsBtn.SetActive(true);
                togglePreviewParts[0].SetActive(false);
                togglePreviewParts[1].SetActive(true);
                presetsUI.gameObject.SetActive(true);
                presetsUI.Init();
                previewUI.SetOff();
            }
            else
            {
                DoneBtn.SetActive(isMyAvatar);
                pictureBtn.SetActive(isMyAvatar);
                bgColorsBtn.SetActive(isMyAvatar);
                togglePreviewParts[0].SetActive(true);
                togglePreviewParts[1].SetActive(false);
                previewUI.Init();
                presetsUI.gameObject.SetActive(false);
                presetsUI.SetOff();
            }
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
            string anim = Data.Instance.characterAnimsManager.defaultEdit.name;
            Events.OnCharacterAnim("", anim);
           // savePanel.SetActive(true);
            //UIManager.Instance.boardUI.SaveWork();
            UIManager.Instance.boardUI.toolsMenu.SetOff();
            isPreview = true;
            SetTogglePreview();
          //  OnActivateUIButtons(false);
           // SetButtons();
        }
        public void SavePart()
        {
            savingPart = true;
            Events.EmptyCharacterItemsButExlude((CharacterPartsHelper.parts)(int)UIManager.Instance.zoomManager.lastZoom);
            UIManager.Instance.zoomManager.ZoomToLastPart();
         //   savePanel.SetActive(false);
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
        }
        void SaveNew()
        {
            savingPart = false;
            Events.SetCharacterIdle("");
          //  savePanel.SetActive(false);
            Data.Instance.charactersData.SetCurrentID("");
            Invoke("SaveWork", 0.1f);
        }
        public void OnSaveClicked()// Guarda la version editada del personaje.
        {
            if (isPreview)
            {
                Save();
            }
            else
            {
                TogglePreview();
                Invoke("Save", 0.5f);
            }
        }
        void Save()
        {
            string chID = Data.Instance.charactersData.GetCurrent();
            CharacterData ch = Data.Instance.charactersData.GetUserCharacter(chID);
            if (ch != null)
            {
                savingPart = false;
                Events.SetCharacterIdle("");
                //   savePanel.SetActive(false);
                SaveWork();
            }
            else
            {
                SaveNew();
            }
        }
        void SaveWork()
        {
            Events.OnNewBodyPartSelected(null);
            UIManager.Instance.boardUI.screenshot.TakeShot(Vector2Int.zero, OnTakeShotDone);
        }
        public void SaveProfilePic()
        {
            print("SaveProfilePic");
            Events.OnBodyPartActive(CharacterPartsHelper.parts.HEAD);
            Events.Zoom(ZoomStates.HEAD, true);
          //  savePanel.SetActive(false);
            Invoke("SaveProfilePicDelayed", 1);
        }
        void SaveProfilePicDelayed()
        {
            UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.charactersData.ThumbSize, SaveProfilePictureDone);
        }
        public void SaveProfilePictureDone(Texture2D tex)
        {
            string thumb = System.Convert.ToBase64String(tex.EncodeToPNG());
            FirebaseStoryMakerDBManager.Instance.SaveProfilePicture(thumb);
        }
       
        bool savingPart = false;
        public void OnTakeShotDone(Texture2D tex)
        {
            UIManager.Instance.workDetailUI.SetTexture(tex);

            if(savingPart)
                Data.Instance.charactersData.SavePartCharacter(tex, (CharacterPartsHelper.parts)(int)UIManager.Instance.zoomManager.currentZoom);
            else
                Data.Instance.charactersData.SaveCharacter(tex, OnSaved);
        }

        private void OnSaved(bool arg1, string arg2)
        {
            Events.OnPopupTopSignalText("Personaje guardado");
        }
    }

}