using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

public class KeyFrameUI : MonoBehaviour
{
    public Sprite sprite;
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
    public float mid;
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
        this.duration = duration;
    }
    public void SetSize(float totalDuration)
    {
        if(r == null)
            r = GetComponent<RectTransform>();
        float w = timeline.total_x_marker * (duration / totalDuration);
        r.sizeDelta = new Vector2(w, r.sizeDelta.y);
        mid = w / 2;
        label.transform.localPosition = new Vector2(mid,0);
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
        //print("UpdateScreenshot " + id);
        if (timeline.filmMakerUI.isEditing)
            Scenario.Instance.TakePartialScreenshot(ScreenshotUpdated);
        else
            screenshot.enabled = false;
    }
    void ScreenshotUpdated(Texture2D texture)
    {
        sprite = Sprite.Create(
            texture,                   
            new Rect(0, 0, texture.width, texture.height), 
            new Vector2(0.5f, 0.5f)   
        );

        screenshot.sprite = sprite;
    }
}
