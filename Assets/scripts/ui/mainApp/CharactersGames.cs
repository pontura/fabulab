using System;
using BoardItems.BoardData;
using UI.MainApp.Home.User;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp
{
    public class CharactersGames : MonoBehaviour
    {
        [SerializeField] CharactersGameScreen charactersGameScreen;
        [SerializeField] Image creatorThumb;
        [SerializeField] TMPro.TMP_Text username;
        void OnEnable()
        {
            Events.OnCharacterReset += OnCharacterReset;
        }
        void OnDisable()
        {
            Events.OnCharacterReset -= OnCharacterReset;            
        }

        private void OnCharacterReset()
        {
            RefreshUser();
        }

        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            charactersGameScreen.Show(true);
            RefreshUser();
        }
        public void RefreshUser()
        {
            string id = Data.Instance.charactersData.GetCurrent();
            CharacterMetaData c = Data.Instance.charactersData.GetMeta(id);
            if(c != null && c.creators.Count>0)
            {
                string creator = c.creators[0];

                  Data.Instance.cacheData.GetUser(creator, (userData, tex) => {
                    username.text = userData.username;
                    creatorThumb.sprite = tex != null ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)) : creatorThumb.sprite;

                });
            }
        }
    }
}
