using BoardItems;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllStoriesScreen : UserStoriesScreen
    {

        bool firstImageCache;
        int count;

        protected override void LoadNext()
        {
            Debug.Log("% AllStoriesScreen LoadNext");
            foreach(FilmDataFabulab cd in Data.Instance.scenesData.filmsData)
            {
                AddFilmMetadata(cd);
            }

            scrollRect.onValueChanged.AddListener(OnScrollChanged);
        }

        protected override void AddFilmMetadata(FilmDataFabulab fd) {
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            go.Init(fd.id, null);
            go.GetComponent<ItemSelectorStory>().SetContent(fd, this, false);
            count++;
            Debug.Log("$ Thumbs Count: " + count);
        }

        string id;
        public override void OpenWork(string id) {
            this.id = id;
            Events.OnLoadingParent(null, LoadingDone);
        }
       
        void LoadingDone()
        {
            Events.OnLoading(true);
            Data.Instance.scenesData.LoadFilm(id);
            UIManager.Instance.boardUI.SetEditingType(BoardUI.editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            Invoke(nameof(SetStoryEditionState), Time.deltaTime * 2);
        }
        
        void SetStoryEditionState() {
            StoryMakerEvents.EnableStoryEdition(false);
            StoryMakerEvents.EnableInputManager(false);
        }

        protected override void LoadImage(int index, ItemSelectorBtn isb) {
            FilmDataFabulab fd = Data.Instance.scenesData.filmsData.Find(x => x.id == isb.Id);
            if (fd!=null) {
                downloading.Add(index);
                Debug.Log($"$ DownloadTexture index: {index} Id: {fd.id}");
                FirebaseStoryMakerDBManager.Instance.DownloadTexture(BoardItems.BoardData.MetadataTypes.stories.ToString(), fd.id, (tex) => {
                    downloading.Remove(index);
                    isb.SetSprite(tex);
                    imageCache[index] = tex;
                    Debug.Log($"ImageCache: {imageCache.Count}");
                    if (!firstImageCache && imageCache.Count >= (visibleRows*itemsPerRows)) {
                        firstImageCache = true;
                        Events.OnLoading(false);
                    }
                }, fd.userID);
            } else {
                Debug.LogError("Couldn´t find Film Metadata with ID " + isb.Id);
            }            
        }
    }

}