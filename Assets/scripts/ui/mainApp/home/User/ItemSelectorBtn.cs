using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class ItemSelectorBtn : SimpleButton
    {
        [SerializeField] CreatorsList creatorList;

        public string Id { get; private set; }

        public void Init(SOPartData cd)
        {
            thumb.sprite = cd.GetSprite();
            Id = cd.id;
        }        

        public void Init(CharacterMetaData cd) {
            thumb.sprite = cd.GetSprite();
            creatorList.Init(cd.creators);
            Id = cd.id;
        }
        public void Init(string id, Sprite sprite) {
            thumb.sprite = sprite;
            Id = id;
        }

        public void SetSprite(Texture2D tex) {
            thumb.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }
    }
}
