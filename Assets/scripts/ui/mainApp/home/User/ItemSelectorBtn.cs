using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class ItemSelectorBtn : MonoBehaviour
    {
        [SerializeField] Image thumb;
        [SerializeField] CreatorsList creatorList;

        public void Init(SOPartData cd)
        {
            thumb.sprite = cd.GetSprite();
        }
        public void Init(Sprite sprite) {
            thumb.sprite = sprite;
        }

        public void Init(CharacterMetaData cd) {
            thumb.sprite = cd.GetSprite();
            creatorList.Init(cd.creators);
        }
    }
}
