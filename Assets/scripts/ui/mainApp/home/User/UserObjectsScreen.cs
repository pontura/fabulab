using BoardItems.BoardData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class UserObjectsScreen : ThumbsScreen
    {
        [SerializeField] protected Button[] buttons;
        [SerializeField] protected SObjectData.types type;
        [SerializeField] protected List<PropMetaData> all;
        [SerializeField] protected Scrollbar scrollbar;
        [SerializeField] protected TagsSelector tagsSelector;

        protected override void Start() {
            base.Start();
            Events.OnPropMetadataUpdated += OnPropMetadataUpdated;
            Events.OnPropMetadataAdded += OnPropMetadataAdded;
            Events.OnPropMetadataRemoved += OnPropMetadataRemoved;
            all = new List<PropMetaData>();
            type = SObjectData.types.generic;
            InitTabs();
            SetType();
            tagsSelector.Init(OnTagSelected);
        }

        private void OnTagSelected(string obj)
        {
            print("Tag seleccionado: " + obj);
            ResetTab();
        }

        public virtual void InitTabs()
        {
            SetTabs();
        }
        protected virtual void OnDestroy() {
            Events.OnPropMetadataUpdated -= OnPropMetadataUpdated;
            Events.OnPropMetadataAdded -= OnPropMetadataAdded;
            Events.OnPropMetadataRemoved -= OnPropMetadataRemoved;
        }

        void OnPropMetadataAdded(PropMetaData fd) {
            if (fd.type == type) {
                AddPropMetadata(fd);
                worksContainer.GetChild(worksContainer.childCount - 1).SetAsFirstSibling();
                if (firstImageCache) {
                    ResetCache();
                }
            }
        }

        protected virtual void AddPropMetadata(PropMetaData fd) {
            if (fd!= null) {
                List<string> tags = fd.tags == null ? new() : fd.tags;
                if (tags.Contains(tagsSelector.SelectedTag()) || tagsSelector.SelectedTag() == Data.Instance.tagsManager.GetNoTagName()) {
                    ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);

                    go.Init(fd, MetadataTypes.so, OpenWork, true);
                    //go.Init(fd, MetadataTypes.so);
                }
            }
        }

        protected virtual void OnPropMetadataUpdated(PropMetaData fd) {
            if (fd.type == type) {
                ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
                ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
                if (btn != null) {
                    btn.Init(fd, MetadataTypes.so);
                    btn.UpdatePublicState();
                    btn.transform.SetAsFirstSibling();
                    ResetCache();
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

        protected override void LoadNext() {
            
            foreach (PropMetaData cd in all)
                AddPropMetadata(cd);

            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }
        public virtual void SetType()
        {
            switch (type)
            {
                default:
                    Data.Instance.sObjectsData.Type = SObjectData.types.generic;
                    all = Data.Instance.sObjectsData.GetUserMetadataByType(SObjectData.types.generic);
                    break;
                case SObjectData.types.background:
                    Data.Instance.sObjectsData.Type = SObjectData.types.background;
                    all = Data.Instance.sObjectsData.GetUserMetadataByType(SObjectData.types.background);
                    break;
            }
        }
        public override void OpenWork(string id)
        {
            UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
        }

        public virtual void New()
        {
            UIManager.Instance.NewCharacter();
        }
        public void TabClicked(int id)
        {
            if(id == 0)
                type = SObjectData.types.generic;
            else
                type = SObjectData.types.background;
            ResetTab();
        }

        void ResetTab() {
            firstLoadDone = false;
            firstImageCache = false;
            imageCache.Clear();
            downloading.Clear();
            SetType();
            SetTabs();
            scrollbar.value = 1f;
            Init();
        }

        void SetTabs()
        {
            switch(type)
            {
                case SObjectData.types.generic:
                    buttons[0].interactable = false;
                    buttons[1].interactable = true;
                    break;
                case SObjectData.types.background:
                    buttons[0].interactable = true;
                    buttons[1].interactable = false;
                    break;
            }
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