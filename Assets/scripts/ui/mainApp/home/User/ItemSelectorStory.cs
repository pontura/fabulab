using BoardItems;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class ItemSelectorStory : ItemSelectorBtn
    {
        [SerializeField] TMPro.TMP_Text titleField;
        [SerializeField] TMPro.TMP_Text creatorField;
        [SerializeField] TMPro.TMP_Text dateField;
        [SerializeField] Image image;


        public void SetContent(FilmDataFabulab content)
        {
            titleField.text =content.name;

            Data.Instance.cacheData.GetUser(content.userID, (userData) => {
                creatorField.text = userData.username;
                image.sprite = userData.thumb != null ? Sprite.Create(userData.thumb, new Rect(0, 0, userData.thumb.width, userData.thumb.height), new Vector2(0.5f, 0.5f)) : null;

            });

            dateField.text = content.GetTimestamp();

        }
    }
}
