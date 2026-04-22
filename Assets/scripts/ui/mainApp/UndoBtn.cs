
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UndoBtn : MonoBehaviour
{
    [SerializeField] Button btn;

    private void Awake()
    {
        Events.ShowUndo += ShowUndo;
        Events.OnUndoAdded += OnUndoAdded;
    }
    private void OnDestroy()
    {
        Events.ShowUndo -= ShowUndo;
        Events.OnUndoAdded -= OnUndoAdded;
    }
    void ShowUndo(bool show)
    {
        print("Show undo " + show);
        gameObject.SetActive(show);
    }
    private void OnUndoAdded()
    {
        btn.interactable = true;
    }
    void OnEnable()
    {
        CheckInteraction();
    }
    void CheckInteraction()
    {
        if(UIManager.Instance!=null)
            btn.interactable = UIManager.Instance.undoManager.undoSteps.Count > 0;
    }
    public void OnClicked()
    {
        UIManager.Instance.undoManager.OnClicked(); 
        CheckInteraction();
    }
}
