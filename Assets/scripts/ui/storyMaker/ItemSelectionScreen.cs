using BoardItems.BoardData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class ItemSelectionScreen : ThumbsScreen
    {
        public bool isOn;
        [SerializeField] Button addBtn;
        [SerializeField] protected Scrollbar scrollbar;

        [field:SerializeField] public bool ReplaceEnabled {  get; set; }

        // Optional: filters the items by tag and name
        public SearchBar searchBar;

        protected override void Start() {
            base.Start();
            if (searchBar != null) {
                searchBar.Init();
                searchBar.OnChanged += OnFilterChanged;
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            if (searchBar != null)
                searchBar.OnChanged -= OnFilterChanged;
        }

        void OnFilterChanged() {
            if (isActive)
                Reload();
        }

        public void AddBtn()
        {
            Button go = Instantiate(addBtn, worksContainer);
            go.GetComponent<Button>().onClick.AddListener(() => New());            
        }

        public override void Show(bool isOn) {
            if (isActive == isOn)
                return;
            isActive = isOn;
            if (isOn)
                Reload();
        }

        void Reload() {
            scrollRect.StopMovement();
            scrollbar.value = 0;
            firstImageCache = false;
            imageCache.Clear();
            downloading.Clear();
            Init();
        }

        protected override void Init() {
           // Events.OnLoadingParent(transform, LoadNext);
            foreach (Transform child in worksContainer) {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }
           LoadNext();
        }
        protected virtual void AddElement(SOPartData part) {
            AddFilteredElement(part.id);
        }
        // Adds the item only if it passes the filter of the search bar
        protected void AddFilteredElement(string id) {
            if (searchBar == null || searchBar.Matches(GetMeta(id)))
                AddElement(id);
        }

        // The metadata of an item (its name and tags), used by the filter
        protected virtual CharacterMetaData GetMeta(string id) {
            return null;
        }

        protected ItemSelectorBtn AddElement(string id) {
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            go.InitForStoryEdition(id, SelectWork);
            go.SetCreator(GetMeta(id));
            return go;
        }

        protected override void LoadNext()
        {
            foreach(SOPartData part in Data.Instance.charactersData.userCharacters)
            {
                AddElement(part);
            }

            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }
        public virtual void New(){
            if (ReplaceEnabled)
                GetComponent<AddNew>().SetActiveButton(0, false);
            else
                GetComponent<AddNew>().SetActiveButton(0, true);
        }

        // Whether the item belongs to the user. Items of the community are loaded before using them
        protected virtual bool IsOwn(string id) {
            return true;
        }

        // A work of another user is used as it is, by its id: it is not copied to the works of the user. The story
        // keeps its own copy of it in storyAssets when it is saved (StoryAssets.Build).
        // It is placed at once, without a loading screen: the scene object loads its content by itself
        // (Items.LoadBoardItemForStory) and shows it when it arrives (Prop / Avatar fit their collider then)
        protected void UseOthersWork(string id) {
            PlaceWork(id);
        }

        protected virtual void SelectWork(string id){
            if (!IsOwn(id)) {
                UseOthersWork(id);
                return;
            }
            PlaceWork(id);
        }

        // Adds the work to the scene, or replaces the selected one with it
        void PlaceWork(string id) {
            if (ReplaceEnabled) {
                Scenario.Instance.sceneObjectsManager.ReplaceSelected(id);
                ReplaceEnabled = false;
            } else
                OpenWork(id);
        }
        
        public override void OpenWork(string id) {
            SOAvatarData data = new SOAvatarData();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
            Scenario.Instance.sceneObjectsManager.AddNewSceneObject(data);
            //UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);    
        }   

        protected override void LoadImage(int index, ItemSelectorBtn isb) {
            /*FilmData fd = Data.Instance.scenesData.filmsData.Find(x => x.id == isb.Id);
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