
using UnityEngine;

public class SaveMenu : MonoBehaviour
{
    [SerializeField] GameObject draftButton;
    [SerializeField] GameObject publicBtn;
    [SerializeField] GameObject gameBtn;
    [SerializeField] TMPro.TMP_Text saveTitle;
    public bool isOn;

    public void Show(bool isOn)
    {   
        this.isOn = isOn;
        gameObject.SetActive(isOn);  
    }

    public void SetPublic(bool isPublic)
    {
        saveTitle.text = "Guardar como...";
        draftButton.SetActive(!isPublic);
        publicBtn.SetActive(true);
        gameBtn.SetActive(false);
    }
    public void SetGame()
    {
        saveTitle.text = "¿Terminaste de jugar?";
        draftButton.SetActive(false);
        publicBtn.SetActive(false);
        gameBtn.SetActive(true);
    }
}
