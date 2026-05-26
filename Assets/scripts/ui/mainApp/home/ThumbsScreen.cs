using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class ThumbsScreen : MonoBehaviour
    {
        [SerializeField] protected ItemSelectorBtn workBtn_prefab;
        [SerializeField] protected Transform worksContainer;

        [SerializeField] bool invertScrollOrder;
        [SerializeField] protected ScrollRect scrollRect;
        [SerializeField] protected int itemsPerRows;
        [SerializeField] protected int visibleRows = 5;
        [SerializeField] protected int cacheSize = 15;
        [SerializeField] protected int cacheExtraItemsCount = 5;
        protected Dictionary<int, Texture2D> imageCache;
        protected HashSet<int> downloading = new HashSet<int>();
        protected bool firstImageCache;
        protected bool firstLoadDone;

        protected virtual void Start() {            
            imageCache = new Dictionary<int, Texture2D>();
        }
        
        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn && !firstLoadDone) {                
                Init();
            }
        }        

        public void Init() {
            Debug.Log("$ Init: " + firstLoadDone);

            if (firstLoadDone)
                return;

            if(Data.Instance.scenesData.filmsData.Count > 0) {
                firstLoadDone = true;
                Events.OnLoadingParent(transform, LoadNext);

                foreach (Transform child in worksContainer) {
                    if (child.tag != "Persistent")
                        Destroy(child.gameObject);
                }
            }
        }        
        
        protected virtual void LoadNext() {
            OnLoadedDone();
        }

        protected virtual void OnLoadedDone()
        {
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
            SetCurrentScrollIndex(scrollRect.normalizedPosition);
            Events.OnLoading(false);
            
        }
                
        public virtual void OpenWork(string id) {
            
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
            float normalizedPos = pos.y; // 0 = inicio, 1 = final
            if (invertScrollOrder)
                normalizedPos = 1f - pos.y; // 0 = inicio, 1 = final
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
            /*FilmDataFabulab fd = Data.Instance.scenesData.filmsData.Find(x => x.id == isb.Id);
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
            }*/
        }

    }

}