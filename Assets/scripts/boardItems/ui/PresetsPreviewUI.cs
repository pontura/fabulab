using BoardItems.UI;
using Common.UI;
using UnityEngine;

public class PresetsPreviewUI : MonoBehaviour
{
    bool isPreview;
    [SerializeField] TMPro.TMP_Text togglePreviewField;
    [SerializeField] PresetsUI presetsUI; 
    [SerializeField] PreviewUI previewUI;

    void OnEnable()
    {
        Invoke("Delayed", 0.1f);
    }
    void Delayed()
    {
        TogglePreview();
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

}
