using BoardItems.BoardData;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllObjectsFullScreenScreen : ThumbsScreen
    {
        [SerializeField] Scrollbar scrollbar;
        SObjectData.types type;

        protected override void Start() {
            base.Start();
            Events.OnPropMetadataUpdated += OnPropMetadataUpdated;
            Events.OnPropMetadataAdded += OnPropMetadataAdded;
            Events.OnPropMetadataRemoved += OnPropMetadataRemoved;
        }

        protected virtual void OnDestroy() {
            Events.OnPropMetadataUpdated -= OnPropMetadataUpdated;
            Events.OnPropMetadataAdded -= OnPropMetadataAdded;
            Events.OnPropMetadataRemoved -= OnPropMetadataRemoved;
        }

        void OnPropMetadataAdded(PropMetaData fd) {
            if (fd.type == type) {
                AddPropMetadata(fd, false);
                worksContainer.GetChild(worksContainer.childCount - 1).SetAsFirstSibling();
            }
        }

        protected void AddPropMetadata(PropMetaData fd, bool userView) {
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            go.Init(fd, MetadataTypes.so);
            go.GetComponent<Button>().onClick.AddListener(() => OpenWork(fd.id));
        }

        void OnPropMetadataUpdated(PropMetaData fd) {
            if (fd.type == type) {
                ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
                ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
                if (btn != null) {
                    btn.Init(fd, MetadataTypes.so);
                    btn.transform.SetAsFirstSibling();
                }
            }
        }

        void OnPropMetadataRemoved(string id) {
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == id);
            if (btn != null) {
                Destroy(btn.gameObject);
            }
        }
                
        public void Init(SObjectData.types type)
        {
            if (this.type == type && isActive)
                return;
            
            this.type = type;

            firstImageCache = false;
            if(imageCache== null) 
                imageCache = new Dictionary<int, Texture2D>();
            imageCache.Clear();
            downloading.Clear();
            scrollbar.value = 1f;
            Init();

            Utils.RemoveAllChildsIn(worksContainer);

            List<PropMetaData> all;
            switch(type)
            {
                case SObjectData.types.generic:
                    Data.Instance.sObjectsData.Type = SObjectData.types.generic;
                    all = Data.Instance.sObjectsData.GetMetadataByType(SObjectData.types.generic);
                    break;
                case SObjectData.types.background:
                    Data.Instance.sObjectsData.Type = SObjectData.types.background;
                    all = Data.Instance.sObjectsData.GetMetadataByType(SObjectData.types.background);
                    break;
                default:
                    all = new List<PropMetaData>();
                    break;
            }
            foreach (PropMetaData cd in all)
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                print("go " + go);
                go.Init(cd, OpenWork);
                AddPropMetadata(cd, true);
            }

            isActive = true;
            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }
       
        public override void OpenWork(string id)
        {
            if(StoryMakerEvents.isEditing)
            {
                switch (type)
                {
                    case SObjectData.types.generic:
                        Events.DuplicateSO(id);
                        break;
                    case SObjectData.types.background:
                        Events.DuplicateSOBG(id);
                        break;
                }
            }
            else
                UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
        }

        protected override void LoadImage(int index, ItemSelectorBtn isb) {
            PropMetaData pMD = Data.Instance.sObjectsData.metaData.Find(x => x.id == isb.Id);
            if (pMD != null) {
                downloading.Add(index);
                Debug.Log($"$ DownloadTexture index: {index} Id: {pMD.id}");
                Data.Instance.cacheData.LoadImage(BoardItems.BoardData.MetadataTypes.so.ToString(), pMD.id, (tex) => {
                    downloading.Remove(index);
                    isb.SetSprite(tex);
                    imageCache[index] = tex;
                    Debug.Log($"ImageCache: {imageCache.Count}");
                    if (!firstImageCache && imageCache.Count >= (cacheSize - cacheExtraItemsCount)) {
                        firstImageCache = true;
                        Events.OnLoading(false);
                    }
                }, pMD.timestamp, pMD.userID);
            } else {
                Debug.LogError("Couldn´t find Film Metadata with ID " + isb.Id);
            }
        }
    }

}