using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

public class AddNew : MonoBehaviour
{
    [SerializeField] Button[] buttons;
    [SerializeField] GameObject panel;
    System.Action<int> OnClicked;
    void Start()
    {
        Close();
        int id = 0;
        foreach (Button btn in buttons)
        {
            int capturedId = id;
            btn.onClick.AddListener(() => Clicked(capturedId));
            id++;
        }
    }
    public void Show(bool isOn, System.Action<int> OnClicked)
    {
        this.OnClicked = OnClicked;
        panel.SetActive(isOn);
    }
    public void Clicked(int id)
    {
        Data.Instance.charactersData.ResetCurrentID();
        StoryMakerEvents.SetEditing(true);
        OnClicked(id);
    }
    public void Close()
    {
        panel.SetActive(false);
    }
    public void Cancel()
    {
        Close();
        GetComponent<UI.MainApp.StoryEditorScreen>().CloseTools();
    }

}
