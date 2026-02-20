using System;
using UI;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class ConfirmationScreen : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text titleField;
    [SerializeField] TMPro.TMP_Text okBtnField;
    [SerializeField] TMPro.TMP_Text cancelBtnField;
    Action<bool> OnDone;

    public void Init()
    {
        Close();
        Events.OnConfirm += OnConfirm;
    }
    void OnDestroy()
    {
        Events.OnConfirm -= OnConfirm;
    }
    private void OnConfirm(string title, string btn1, string btn2, Action<bool> OnDone)
    {
        print("OnConfirm ");
        titleField.text = title;
        okBtnField.text = btn1;
        cancelBtnField.text = btn2;
        this.OnDone = OnDone;
        gameObject.SetActive(true);
    }
    public void Yes()
    {
        OnDone(true);
        UIManager.Instance.boardUI.ResetBoardConfirmed();
        Close();
    }
    public void No()
    {
        OnDone(false);
        Close();
    }
    void Close()
    {
        gameObject.SetActive(false);
    }
    
}
