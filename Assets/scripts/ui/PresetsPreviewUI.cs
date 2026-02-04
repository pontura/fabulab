using BoardItems;
using BoardItems.UI;
using System;
using UnityEngine;

namespace UI
{
    public class PresetsPreviewUI : MonoBehaviour
    {
        bool isPreview;
        [SerializeField] GameObject savePanel;
        [SerializeField] TMPro.TMP_Text togglePreviewField;
        [SerializeField] PresetsUI presetsUI;
        [SerializeField] PreviewUI previewUI;
        public GameObject DoneBtn;

        private void Start()
        {
            Events.ActivateUIButtons += OnActivateUIButtons;
        }
        private void OnDestroy()
        {
            Events.ActivateUIButtons -= OnActivateUIButtons;
        }       

        void OnEnable()
        {
            savePanel.SetActive(false);
            OnActivateUIButtons(false);
            Invoke("Delayed", 0.1f);
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
        }
        public void Cancel()
        {
        }

    }

}