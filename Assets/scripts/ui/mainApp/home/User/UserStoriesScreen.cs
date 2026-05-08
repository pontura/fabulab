using BoardItems;
using System;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.DB;
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
            Debug.Log("% UserStoriesScreen OnFilmMetadataAdded");
            AddFilmMetadata(fd);
            worksContainer.GetChild(worksContainer.childCount - 1).SetAsFirstSibling();
        }

        void OnFilmMetadataUpdated(FilmDataFabulab fd) {
            Debug.Log("% UserStoriesScreen OnFilmMetadataUpdated");
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                btn.Init(fd.GetSprite());
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

        public void Init() {
            Events.OnLoadingParent(transform, LoadNext);
            artID = 0;

            foreach (Transform child in worksContainer) {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }
        }        
        
        protected virtual void LoadNext()
        {
            foreach(FilmDataFabulab cd in Data.Instance.scenesData.userFilmsData)
            {
                AddFilmMetadata(cd);                
            }
            if (Data.Instance.scenesData.userFilmsData.Count > 0) {
                firstLoad = true;
                Events.OnLoading(false);
            }
        }

        protected virtual void AddFilmMetadata(FilmDataFabulab fd) {
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
            Events.OnConfirm("Confirmás que querés borrar esta historia?", "SI", "NO", OnConfirm);
        }
        void OnConfirm(bool ok) {
            if (ok) {
                FirebaseStoryMakerDBManager.Instance.DeleteFilm(Data.Instance.scenesData.userFilmsData.Find(x => x.id == id), OnDeleted);
                this.id = null;
            }
        }
        public void OnDeleted(string filmId) {            
            Data.Instance.scenesData.RemoveFD(filmId);
            Events.OnFilmMetadataRemoved(filmId);
        }
    }

}