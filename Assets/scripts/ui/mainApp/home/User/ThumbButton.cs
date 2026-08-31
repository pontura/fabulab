using BoardItems;
using UnityEngine;
using UnityEngine.UI;

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
        LoadImage(id);
    }
    public void OnClicked()
    {
        OnClick(id);
    }
     protected void LoadImage(string id) {
        FilmDataFabulab fd = Data.Instance.scenesData.filmsData.Find(x => x.id == id);
        if (fd != null) {
            Data.Instance.cacheData.LoadImage(BoardItems.BoardData.MetadataTypes.stories.ToString(), fd.id, SetTexure, fd.timestamp, fd.userID);
        } else {
            Debug.LogError("Couldn´t find Film Metadata with ID " + id);
        }
    }
    void SetTexure(Texture2D  tex)
    {
        thumb.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        loading.SetActive(false);
        loading.SetActive(false);
    }
}
