using UnityEngine;
using UnityEngine.UI;

public class AddNew : MonoBehaviour
{
    [SerializeField] Button[] buttons;
    [SerializeField] GameObject panel;

    void Start()
    {
        Close();
        int id = 0;
        foreach (Button btn in buttons)
        {
            int capturedId = id; // 👈 copia local
            btn.onClick.AddListener(() => Clicked(capturedId));
            id++;
        }
    }
    public void Show(bool isOn)
    {
        panel.SetActive(isOn);
    }
    public void Clicked(int id)
    {
        print("id: " + id);
    }  
    void Close()
    {
        panel.SetActive(false);
        GetComponent<UI.MainApp.StoryEditorScreen>().CloseTools();
    }
    public void Cancel()
    {
        Close();
    }
   
}
