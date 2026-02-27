using UnityEditor.Presets;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace UI.MainApp
{
    public class ObjectsEdition : MainScreen
    {
        [SerializeField] Transform dragAndDropContainer;
        [SerializeField] DragAndDropUI dragAndDropUI;
        [SerializeField] GameObject savePanel;
        [SerializeField] GameObject saveButton;
        [SerializeField] GameObject replaceButton;
        [SerializeField] GameObject DoneBtn;
        [SerializeField] bool changesMade;
        [SerializeField] ToggleButton snapToggle;

        void OnToggle(bool isOn)
        {
            UIManager.Instance.boardUI.snap = isOn;
        }
        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Creation_Objects:
                    snapToggle.Init(OnToggle, UIManager.Instance.boardUI.snap);
                    Events.Zoom(BoardItems.Characters.CharacterPartsHelper.parts.BODY, false);
                    changesMade = false;
                    SetButtons();
                    Show(true);
                    savePanel.SetActive(false);
                    dragAndDropUI.SetOn(true);
                    dragAndDropUI.transform.SetParent(dragAndDropContainer);
                    dragAndDropUI.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                    dragAndDropUI.GetComponent<RectTransform>().anchorMax = Vector2.one;
                    dragAndDropUI.GetComponent<RectTransform>().offsetMin = Vector2.zero; 
                    dragAndDropUI.GetComponent<RectTransform>().offsetMax = Vector2.zero;  
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
           UIManager.Instance.boardUI.screenshot.TakeShot(Data.Instance.charactersData.thumbSize, OnTakeShotDone);
        }
        public void OnTakeShotDone(Texture2D tex)
        {
            Data.Instance.sObjectsData.SaveSO(tex);
        }

    }

}