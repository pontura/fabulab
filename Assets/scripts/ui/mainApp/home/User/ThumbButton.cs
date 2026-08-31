using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class ThumbButton : MonoBehaviour
{

    [SerializeField] Image thumb;
    [SerializeField] GameObject loading;
    System.Action<string> OnClick;
    string id;

    public void Init(string id, System.Action<string> OnClick)
    {
        this.id = id;
        this.OnClick = OnClick;
    }
    public void OnClicked()
    {
        OnClick(id);
    }
}
