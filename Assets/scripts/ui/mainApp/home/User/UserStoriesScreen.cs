using BoardItems;
using System;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class UserStoriesScreen : MonoBehaviour
    {
        public ItemSelectorBtn workBtn_prefab;
        public Transform worksContainer;

        protected int artID = 0;
        protected bool firstLoad;

        protected virtual void Start() {
            Events.OnFilmMetadataUpdated += OnFilmMetadataUpdated;
            Events.OnFilmMetadataAdded += OnFilmMetadataAdded;
            Events.OnFilmMetadataRemoved += OnFilmMetadataRemoved;
        }
        protected virtual void OnDestroy() {
            Events.OnFilmMetadataUpdated += OnFilmMetadataUpdated;
            Events.OnFilmMetadataAdded -= OnFilmMetadataAdded;
            Events.OnFilmMetadataRemoved += OnFilmMetadataRemoved;
        }
        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn && !firstLoad) {                
                Init();
            }
        }

        void OnFilmMetadataAdded(FilmDataFabulab fd) {
            AddFilmMetadata(fd);
            worksContainer.GetChild(worksContainer.childCount - 1).SetAsFirstSibling();
        }

        void OnFilmMetadataUpdated(FilmDataFabulab fd) {
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                btn.Init(fd.GetSprite());
                btn.transform.SetAsFirstSibling();
            }
        }

        void OnFilmMetadataRemoved(string id) {
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == id);
            if (btn != null) {
                Destroy(btn.gameObject);
            }
        }

        public void Init()
        {
            artID = 0;

            foreach (Transform child in worksContainer)
            {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }

            LoadNext();
        }
        
        protected virtual void LoadNext()
        {
            foreach(FilmDataFabulab cd in Data.Instance.scenesData.userFilmsData)
            {
                AddFilmMetadata(cd);                
            }
            if (Data.Instance.scenesData.userFilmsData.Count > 0)
                firstLoad = true;
        }

        void AddFilmMetadata(FilmDataFabulab fd) {
            Debug.Log("% OnFilmMetadataAdded");
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            go.Init(fd.id, fd.GetSprite());
            go.GetComponent<ItemSelectorStory>().SetContent(fd, this, true);
        }
        string id;
        public virtual void OpenWork(string id) {
            this.id = id;
            Events.OnLoadingParent(null, LoadingDone);
        }
        void LoadingDone()
        {
            Events.OnLoading(true);
            Data.Instance.scenesData.LoadUserFilm(id);
            UIManager.Instance.boardUI.SetEditingType(BoardUI.editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            Invoke(nameof(SetUserStoryEditionState), Time.deltaTime * 2);
        }

        void SetUserStoryEditionState() {
            StoryMakerEvents.EnableStoryEdition(true);
        }
        public void Duplicate(string id)
        {
            this.id = id;
            print("Duplica ID: " + id);
        }
        public void Delete(string id)
        {
            this.id = id;
            print("Delete ID: " + id);
        }
    }

}