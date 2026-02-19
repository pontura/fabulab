using System;
using UI;
using UnityEngine;

public class ConfirmationScreen : MonoBehaviour
{
    public GameObject panel;
    [SerializeField] TMPro.TMP_Text titleField;
    [SerializeField] TMPro.TMP_Text okBtnField;
    [SerializeField] TMPro.TMP_Text cancelBtnField;

    void Start()
    {
        Close();
        Events.OnConfirm += OnConfirm;
    }
    void OnDestroy()
    {
        Events.OnConfirm -= OnConfirm;
    }
    private void OnConfirm(string title, string btn1, string btn2, Action<bool> action)
    {
        panel.SetActive(true);
    }
    public void Yes()
    {
        GetComponent<BoardUI>().ResetBoardConfirmed();
        Close();
    }
    void Close()
    {
        GetComponent<BoardUI>().ResetBoardConfirmed();
    }
    public void No()
    {
        panel.SetActive(false);
    }
}
