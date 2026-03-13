using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

public class EditFieldUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMPro.TMP_InputField field;
    ObjectSignal objectSignal;

    void Start() {
        Close();
        StoryMakerEvents.OnInputField += OnInputField;
    }
    private void OnDestroy() {
        StoryMakerEvents.OnInputField -= OnInputField;
    }
    void SetFocus() {
        field.Select();
    }

    void OnInputField(ObjectSignal objectSignal) {
        Invoke("SetFocus", 0.1f);
        this.objectSignal = objectSignal;
        if (objectSignal.field.text != "")
            field.text = objectSignal.field.text;
        panel.SetActive(true);
    }

    public void Done() {
        string text = field.text;

        if (text == "") {
            //Events.OnUiSfx(UiSfxManager.UiSfxType.WRONG);
            return;
        }
        /*Events.OnUiSfx(UiSfxManager.UiSfxType.OK);
        if (objectSignal == null) {
            if (OnDone != null)
                OnDone(text);
        } else*/
            objectSignal.SetField(text);
        Close();
    }

    void Close() {
        panel.SetActive(false);
    }
    public void Cancel() {
        //Events.OnUiSfx(UiSfxManager.UiSfxType.CANCEL);
        Close();
    }
}
