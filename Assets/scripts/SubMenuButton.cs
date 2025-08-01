using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubMenuButton : MonoBehaviour
{
    int id;
    System.Action<int> OnClick;
    [SerializeField] private Image image;

    public void Init(int id, Sprite sprite, System.Action<int> OnClick, List<Color> colors)
    {
        this.id = id;
        this.OnClick = OnClick;
        //image.sprite = sprite;
        image.color = colors[0];
    }
    public void Init(int id, Sprite sprite, System.Action<int> OnClick)
    {
        this.id = id;
        this.OnClick = OnClick;
        image.sprite = sprite;
        if(id == 0)
            image.color = Color.white;
    }
    public void Clicked()
    {
        AudioManager.Instance.uiSfxManager.PlayNextScale("click");
        this.OnClick(id);
    }
}
