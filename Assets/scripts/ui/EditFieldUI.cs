using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

public class EditFieldUI : MonoBehaviour
{
    [SerializeField] GameObject[] fontsBtn;
    [SerializeField] GameObject panel;
    [SerializeField] TMPro.TMP_InputField field;
    ObjectSignal objectSignal;

    void Start() {
        Close();
        StoryMakerEvents.OnInputField += OnInputField;
        StoryMakerEvents.ShowSoButtons += ShowSoButtons;
        int id = 0;
        foreach (GameObject btn in fontsBtn)
        {
            int capturedId = id; // 👈 copia local
            btn.GetComponent<Button>().onClick.AddListener(() => FontClicked(capturedId));
            id++;
        }
    }
    private void OnDestroy() {
        StoryMakerEvents.OnInputField -= OnInputField;
        StoryMakerEvents.ShowSoButtons -= ShowSoButtons;
    }
    void ShowSoButtons(Vector3 pos, SOData data)
    {
        print("ShowSoButtons " + data);
        if (data is SOInputData)
            OnInputField((ObjectSignal)Scenario.Instance.sceneObejctsManager.selected);
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
    public void OnFieldChanged()
    {
        objectSignal.SetField(field.text);
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

        Debug.Log("&& : "+text);
        objectSignal.SetField(text);
        Close();
    }
    public void ClosePanel()
    {
        panel.SetActive(false);
    }
    void Close() {
        ClosePanel();
        GetComponent<UI.MainApp.StoryEditorScreen>().CloseTools();
    }
    public void Cancel() {
        //Events.OnUiSfx(UiSfxManager.UiSfxType.CANCEL);
        Close();
    }
    public void Delete()
    {
        StoryMakerEvents.RemoveSceneObject();
        Close();
    }
    public void FontClicked(int id)
    {
        print("id: " + id);
        Fonts font = (Scenario.Instance.sceneObejctsManager as SceneObjectsManagerFabulab).FontAssets.GetFont(id);
        field.fontAsset = font.fontAsset;
        StoryMakerEvents.ChangeFont(id);

    }
}
