using BoardItems;
using BoardItems.Characters;
using Common.UI;
using System;
using UnityEngine;
using static BoardItems.Characters.CharacterPartsHelper;

namespace UI.MainApp
{
    public class ObjectsEdition : MainScreen
    {
        [SerializeField] SceneObjectsPanel sceneObjectsPanel;
        [SerializeField] DragAndDropUI dragAndDropUI;
        [SerializeField] GameObject savePanel;
        [SerializeField] GameObject saveButton;
        [SerializeField] GameObject replaceButton;
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
                            break;
                        case BoardItems.BoardData.SObjectData.types.background:
                            Events.Zoom(ZoomStates.BACKGROUND, false);
                            break;
                    }
                    UIManager.Instance.boardUI.activeBoardItem.Init();
                    changesMade = false;
                    SetButtons();
                    Show(true);
                    savePanel.SetActive(false);
                    dragAndDropUI.SetOn(true);
                    dragAndDropUI.Init();
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
        public void SetButtons() {
            saveButton.SetActive(true);
            replaceButton.SetActive(Data.Instance.sObjectsData.GetCurrent() != "");
        }
        public void Save()
        {
            savePanel.SetActive(false);
            Data.Instance.sObjectsData.SetCurrentID("");// Resetea si hay un character elegido.
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
            Events.OnNewBodyPartSelected(null);
            UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.charactersData.thumbSize, OnTakeShotDone);
        }
        public void OnTakeShotDone(Texture2D tex)
        {
            Data.Instance.sObjectsData.SaveSO(tex);
        }
       
    }

}