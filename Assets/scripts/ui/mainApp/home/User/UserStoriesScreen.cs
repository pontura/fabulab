using BoardItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;
using static UnityEditor.PlayerSettings;

namespace UI.MainApp.Home.User
{
    public class UserStoriesScreen : MonoBehaviour
    {
        public ItemSelectorBtn workBtn_prefab;
        public Transform worksContainer;

        [SerializeField] protected ScrollRect scrollRect;
        [SerializeField] protected int itemsPerRows;
        [SerializeField] protected int visibleRows = 5;
        [SerializeField] protected int cacheSize = 15;
        [SerializeField] protected int cacheExtraItemsCount = 5;
        protected Dictionary<int, Texture2D> imageCache;
        protected HashSet<int> downloading = new HashSet<int>();

        protected int artID = 0;
        protected bool firstLoad;

        protected bool firstImageCache;
        protected int count;

        protected virtual void Start() {
            Events.OnFilmMetadataUpdated += OnFilmMetadataUpdated;
            Events.OnFilmMetadataAdded += OnFilmMetadataAdded;
            Events.OnFilmMetadataRemoved += OnFilmMetadataRemoved;

            imageCache = new Dictionary<int, Texture2D>();
        }
        protected virtual void OnDestroy() {
            Events.OnFilmMetadataUpdated -= OnFilmMetadataUpdated;
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
            Debug.Log("$ Init: " + firstLoad);
            if(Data.Instance.scenesData.filmsData.Count > 0) {
                firstLoad = true;
                Events.OnLoadingParent(transform, LoadNext);
                artID = 0;

                foreach (Transform child in worksContainer) {
                    if (child.tag != "Persistent")
                        Destroy(child.gameObject);
                }
            }
        }        
        
        protected virtual void LoadNext()
        {
            Debug.Log("$ LoadNext ");
            foreach (FilmDataFabulab cd in Data.Instance.scenesData.userFilmsData)
            {
                AddFilmMetadata(cd);                
            }
            
            SetCurrentScrollIndex(scrollRect.normalizedPosition);
            Events.OnLoading(false);
            
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

        protected void OnScrollChanged(Vector2 pos) {
            // Calcular el índice actual visible según posición del scroll
            SetCurrentScrollIndex(pos);
        }

        void SetCurrentScrollIndex(Vector2 pos) {
            int currentIndex = CalculateCurrentIndex(pos);
            UpdateCacheOnScroll(currentIndex);
        }

        int CalculateCurrentIndex(Vector2 pos) {
            // Ejemplo simple para scroll horizontal:
            Debug.Log("$ " + pos);
            float normalizedPos = 1f - pos.y; // 0 = inicio, 1 = final
            Debug.Log($"$ {normalizedPos} * ({worksContainer.childCount}  - ({visibleRows} * {itemsPerRows}))");
            int currentIndex = Mathf.FloorToInt(normalizedPos * (worksContainer.childCount - (visibleRows * itemsPerRows)));
            return Mathf.Clamp(currentIndex, 0, (worksContainer.childCount - (visibleRows * itemsPerRows)));
        }

        void UpdateCacheOnScroll(int currentIndex) {
            

            // Calcular rango simétrico: 5 antes + visibles + 5 después
            int startIndex = Mathf.Max(0, currentIndex - cacheExtraItemsCount);
            int endIndex = Mathf.Min(worksContainer.childCount - 1, currentIndex + (visibleRows * itemsPerRows) - 1 + cacheExtraItemsCount);

            Debug.Log($"$ startIndex: {startIndex} endIndex: {endIndex}");

            // Eliminar las que ya no están en rango
            var keysToRemove = imageCache.Keys
                .Where(k => k < startIndex || k > endIndex)
                .ToList();

            foreach (var key in keysToRemove) {
                Destroy(imageCache[key]);
                imageCache.Remove(key);
            }

            // Cargar nuevas imágenes en el rango
            for (int i = startIndex; i <= endIndex; i++) {
                if (!imageCache.ContainsKey(i) && !downloading.Contains(i)) {
                    ItemSelectorBtn isb = worksContainer.GetChild(i).GetComponent<ItemSelectorBtn>();
                    if (isb != null) {
                        LoadImage(i, isb);
                    } else {
                        Debug.LogError("Couldn´t find ItemSelectorBtn with index " + i);
                    }
                }
            }
        }

        protected virtual void LoadImage(int index, ItemSelectorBtn isb) {
            FilmDataFabulab fd = Data.Instance.scenesData.filmsData.Find(x => x.id == isb.Id);
            if (fd != null) {
                downloading.Add(index);
                Debug.Log($"$ DownloadTexture index: {index} Id: {fd.id}");
                Data.Instance.cacheData.LoadImage(BoardItems.BoardData.MetadataTypes.stories.ToString(), fd.id, (tex) => {
                    downloading.Remove(index);
                    isb.SetSprite(tex);
                    imageCache[index] = tex;
                    Debug.Log($"ImageCache: {imageCache.Count}");
                    if (!firstImageCache && imageCache.Count >= (visibleRows * itemsPerRows)) {
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