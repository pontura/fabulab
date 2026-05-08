using BoardItems;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class ItemSelectorStory : ItemSelectorBtn
    {
        [SerializeField] TMPro.TMP_Text titleField;
        [SerializeField] TMPro.TMP_Text creatorField;
        [SerializeField] TMPro.TMP_Text dateField;
        [SerializeField] Image creatorThumb;

        [SerializeField] GameObject editionGO;
        [SerializeField] GameObject viewGO;
        UserStoriesScreen userStoriesScreen;
        FilmDataFabulab content;

        public void SetContent(FilmDataFabulab content, UserStoriesScreen userStoriesScreen, bool isEdition)
        {
            this.content = content;
            this.userStoriesScreen = userStoriesScreen;
            titleField.text =content.name;
            InitStoryBtn(isEdition);
            dateField.text = content.GetTimestamp();
        }

        void InitStoryBtn(bool isEdition)
        {
            print("InitStoryBtn" + content.userID + " u: "  + Data.Instance.userData.userDataInDatabase.uid);
            if (isEdition)
            {
                editionGO.SetActive(true);
                viewGO.SetActive(false);
            }
            else
            {
                GetComponent<Button>().onClick.AddListener(() => userStoriesScreen.OpenWork(content.id));
                editionGO.SetActive(false);
                viewGO.SetActive(true);

                Data.Instance.cacheData.GetUser(content.userID, (userData) => {
                    creatorField.text = userData.username;
                    creatorThumb.sprite = userData.thumb != null ? Sprite.Create(userData.thumb, new Rect(0, 0, userData.thumb.width, userData.thumb.height), new Vector2(0.5f, 0.5f)) : null;

                });
            }
        }
        public void Edit()
        {
            userStoriesScreen.OpenWork(content.id);
        }
        public void Duplicate()
        {
            userStoriesScreen.Duplicate(content.id);
        }
        public void Delete()
        {
            userStoriesScreen.Delete(content.id);
        }
    }
}
