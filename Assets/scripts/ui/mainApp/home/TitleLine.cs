using UnityEngine;

public class TitleLine : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text field;
    public void Init(string title)
    {
        field.text = title;
    }
}
