
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UndoBtn : MonoBehaviour
{
    [SerializeField] Button btn;

    private void Start()
    {
        Events.OnUndoAdded += OnUndoAdded;
    }
    private void OnDestroy()
    {
        Events.OnUndoAdded -= OnUndoAdded;
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
        btn.interactable = UIManager.Instance.undoManager.undoSteps.Count > 0;
    }
    public void OnClicked()
    {
        UIManager.Instance.undoManager.OnClicked(); 
        CheckInteraction();
    }
}
