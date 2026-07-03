using BoardItems;
using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.DB;

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

        override public void Init(string id, Sprite sprite) {
            base.Init(id, sprite);
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
            metadataType = MetadataTypes.stories;
        }

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
            if (isEdition) {
                editionGO.SetActive(true);
                viewGO.SetActive(false);
                bool isPublic = Data.Instance.scenesData.GetMeta(Id).isPublic;
                infoBtn.gameObject.SetActive(true);
                infoBtn.Init(OnInfoClicked, isPublic);
            } else {
                GetComponent<Button>().onClick.AddListener(() => {
                    AudioManager.Instance.uiSfxManager.PlayTransp("click", 5);
                    userStoriesScreen.OpenWork(content.id); 
                });
                editionGO.SetActive(false);
                viewGO.SetActive(true);

                Data.Instance.cacheData.GetUser(content.userID, (userData, tex) => {
                    creatorField.text = userData.username;
                    creatorThumb.sprite = tex != null ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)) : null;

                });
            }
        }
        public void Edit()
        {
            AudioManager.Instance.uiSfxManager.PlayTransp("click", 5);
            userStoriesScreen.OpenWork(content.id);
        }
        public void Duplicate()
        {
            AudioManager.Instance.uiSfxManager.PlayTransp("click", 5);
            userStoriesScreen.Duplicate(content.id);
        }
        public override void Delete()
        {
            AudioManager.Instance.uiSfxManager.PlayTransp("click", -5);
            print("Delete ID: " + content.id);
            Events.OnConfirm("Confirmás que querés borrar esta historia?", "SI", "NO", OnConfirm);
        }
        protected override void OnConfirm(bool ok) {
            if (ok) {
                FirebaseStoryMakerDBManager.Instance.DeleteFilm(Data.Instance.scenesData.filmsData.Find(x => x.id == content.id), OnDeleted);
            }
        }
        protected override void OnDeleted(string filmId) {
            Data.Instance.scenesData.RemoveFD(filmId);
            Events.OnFilmMetadataRemoved(filmId);
            Destroy(gameObject);
        }
    }
}
