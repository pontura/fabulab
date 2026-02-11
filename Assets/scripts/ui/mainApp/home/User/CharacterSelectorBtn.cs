using BoardItems;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class CharacterSelectorBtn : MonoBehaviour
    {
        [SerializeField] Image thumb;
        public void Init(AlbumData.CharacterData cd)
        {
            thumb.sprite = cd.GetSprite();
        }
    }
}
