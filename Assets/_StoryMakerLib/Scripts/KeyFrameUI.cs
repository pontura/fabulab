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
    [SerializeField] Image marker;
    [SerializeField] Image screenshot;
    public bool selected;
    int id = 0;
    Timeline timeline;
    public float duration;

    public void Init(Timeline timeline)
    {
        marker.color = selectedColor;
        this.timeline = timeline;
    }
    public void SetID(int id)
    {
        this.id = id;
        field.text = (id).ToString();
    }
    public void SetDuration(float duration)
    {
        print("SetDuration duration: " + duration);
        this.duration = duration;
    }
    public void SetSize(float totalDuration)
    {
        if(r == null)
            r = GetComponent<RectTransform>();
        float w = timeline.total_x_marker * (duration / totalDuration);
        r.sizeDelta = new Vector2(w, r.sizeDelta.y);
        float _x = w / 2;
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
        marker.enabled = isOn;
    }
    public void OnClick()
    {
        timeline.SetJump(id);
    }
    public void UpdateScreenshot()
    {
        print("UpdateScreenshot " + id);
        Scenario.Instance.Screenshot(ScreenshotUpdated);
    }
    void ScreenshotUpdated(Texture2D texture)
    {
        Sprite sprite = Sprite.Create(
            texture,                   
            new Rect(0, 0, texture.width, texture.height), 
            new Vector2(0.5f, 0.5f)   
        );

        screenshot.sprite = sprite;
    }
}
