using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

public class KeyFrameUI : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text field;
    [SerializeField] GameObject label;

    [SerializeField] Color selectedColor;
    [SerializeField] Color color1;
    [SerializeField] Color color2;
    Color color;

    RectTransform r;
    [SerializeField] Image image;
    public bool selected;
    int id = 0;
    Timeline timeline;

    public void Init(Timeline timeline)
    {
        this.timeline = timeline;
    }
    public void SetID(int id)
    {
        this.id = id;
        field.text = (id).ToString();   
    }
    public void SetSize(float size)
    {
        if(r == null)
            r = GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(size, r.sizeDelta.y);
        float _x = size / 2;
        label.transform.localPosition = new Vector2(_x,0);
    }
    public void SetColor(bool pair)
    {
        if (pair)
            image.color = color1;
        else
            image.color = color2;

        this.color = image.color;
    }
    public void SetSelected(bool isOn)
    {
        this.selected = isOn;
        if (isOn)
            image.color = selectedColor;
        else
            image.color = color;
    }
    public void OnClick()
    {
        timeline.JumpTo(id);
    }
}
