using BoardItems;
using System;
using System.Collections.Generic;
using UnityEngine;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class UserStoriesScreen : ThumbsScreen
    {

        protected override void Start() {
            base.Start();
            Events.OnFilmMetadataUpdated += OnFilmMetadataUpdated;
            Events.OnFilmMetadataAdded += OnFilmMetadataAdded;
            Events.OnFilmMetadataRemoved += OnFilmMetadataRemoved;

            imageCache = new Dictionary<int, Texture2D>();
        }
        protected virtual void OnDestroy() {
            Events.OnFilmMetadataUpdated -= OnFilmMetadataUpdated;
            Events.OnFilmMetadataAdded -= OnFilmMetadataAdded;
            Events.OnFilmMetadataRemoved -= OnFilmMetadataRemoved;
        }        

        void OnFilmMetadataAdded(FilmDataFabulab fd) {
            Debug.Log("% UserStoriesScreen OnFilmMetadataAdded");
            AddFilmMetadata(fd);
            worksContainer.GetChild(worksContainer.childCount - 1).SetAsFirstSibling();
        }

        protected virtual void OnFilmMetadataUpdated(FilmDataFabulab fd) {
            Debug.Log("% UserStoriesScreen OnFilmMetadataUpdated "+gameObject.name);
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                btn.Init(fd.GetSprite());
                btn.UpdatePublicState();
                btn.transform.SetAsFirstSibling();
            }
        }

        void OnFilmMetadataRemoved(string id) {
            Debug.Log("% UserStoriesScreen OnFilmMetadataUpdated");
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == id);
            if (btn != null) {
                Debug.Log("% UserStoriesScreen OnFilmMetadataUpdated destroy");
                Destroy(btn.gameObject);
            }
        }
        
        protected override void LoadNext()
        {
            Debug.Log("$ LoadNext ");
            foreach (FilmDataFabulab cd in Data.Instance.scenesData.userFilmsData)
            {
                AddFilmMetadata(cd);                
            }

            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);

        }

        protected virtual void AddFilmMetadata(FilmDataFabulab fd) {
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            go.Init(fd.id, null);
            go.GetComponent<ItemSelectorStory>().SetContent(fd, this, true);
        }
        string iD;
        public override void OpenWork(string id) {
            this.iD = id;
            Events.OnLoadingParent(null, LoadingWorkDone);
        }
        void LoadingWorkDone()
        {
            Events.OnLoading(true);
            Data.Instance.scenesData.LoadUserFilm(iD);
            UIManager.Instance.boardUI.SetEditingType(BoardUI.editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            Invoke(nameof(SetUserStoryEditionState), Time.deltaTime * 2);
        }

        void SetUserStoryEditionState() {
            StoryMakerEvents.EnableStoryEdition(true);
        }
        public void Duplicate(string id)
        {
            this.iD = id;
            print("Duplica ID: " + id);
        }
        
        protected override void LoadImage(int index, ItemSelectorBtn isb) {
            FilmDataFabulab fd = Data.Instance.scenesData.filmsData.Find(x => x.id == isb.Id);
            if (fd != null) {
                downloading.Add(index);
                Debug.Log($"$ DownloadTexture index: {index} Id: {fd.id}");
                Data.Instance.cacheData.LoadImage(BoardItems.BoardData.MetadataTypes.stories.ToString(), fd.id, (tex) => {
                    downloading.Remove(index);
                    isb.SetSprite(tex);
                    imageCache[index] = tex;
                    Debug.Log($"ImageCache: {imageCache.Count}");
                    if (!firstImageCache && imageCache.Count >= (cacheSize - cacheExtraItemsCount)) {
                        firstImageCache = true;
                        Events.OnLoading(false);
                    }
                }, fd.timestamp, fd.userID);
            } else {
                Debug.LogError("Couldn´t find Film Metadata with ID " + isb.Id);
            }
        }

    }

}