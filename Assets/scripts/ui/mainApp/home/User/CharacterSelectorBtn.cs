using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class CharacterSelectorBtn : MonoBehaviour
    {
        [SerializeField] Image thumb;
        [SerializeField] CreatorsList creatorList;

        public void Init(CharacterData cd)
        {
            thumb.sprite = cd.GetSprite();
        }

        public void Init(CharacterMetaData cd) {
            thumb.sprite = cd.GetSprite();
            creatorList.Init(cd.creators);
        }
    }
}
