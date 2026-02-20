using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class CharacterSelectorBtn : MonoBehaviour
    {
        [SerializeField] Image thumb;
        public void Init(CharacterData cd)
        {
            thumb.sprite = cd.GetSprite();
        }
    }
}
