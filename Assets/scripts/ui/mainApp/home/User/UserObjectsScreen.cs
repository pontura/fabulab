using BoardItems.BoardData;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class UserObjectsScreen : MonoBehaviour
    {
        public ItemSelectorBtn workBtn_prefab;
        public Button[] buttons;
        public SObjectData.types type;
        public List<PropMetaData> all;

        public Transform container;

        protected bool firstLoad;

        protected virtual void Start() {
            Events.OnPropMetadataUpdated += OnPropMetadataUpdated;
            Events.OnPropMetadataAdded += OnPropMetadataAdded;
            Events.OnPropMetadataRemoved += OnPropMetadataRemoved;
            InitTabs();
        }
        public virtual void InitTabs()
        {
            SetTabs();
        }
        protected virtual void OnDestroy() {
            Events.OnPropMetadataUpdated += OnPropMetadataUpdated;
            Events.OnPropMetadataAdded -= OnPropMetadataAdded;
            Events.OnPropMetadataRemoved += OnPropMetadataRemoved;
        }

        void OnPropMetadataAdded(PropMetaData fd) {
            AddPropMetadata(fd);
            container.GetChild(container.childCount - 1).SetAsFirstSibling();
        }

        protected void AddPropMetadata(PropMetaData fd) {
            ItemSelectorBtn go = Instantiate(workBtn_prefab, container);
            go.Init(fd);
            go.GetComponent<Button>().onClick.AddListener(() => OpenWork(fd.id));
        }

        void OnPropMetadataUpdated(PropMetaData fd) {
            ItemSelectorBtn[] itemBtns = container.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                btn.Init(fd);
                btn.transform.SetAsFirstSibling();
            }
        }

        void OnPropMetadataRemoved(PropMetaData fd) {
            ItemSelectorBtn[] itemBtns = container.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                Destroy(btn.gameObject);
            }
        }

        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn && !firstLoad) {
                Init();
            }
        }
        public virtual void Init()
        {
            type = SObjectData.types.generic;
            SetTabs();
            Events.OnLoadingParent(container.parent, LoadingDone);
        }
        void LoadingDone()
        {
            Events.OnLoading(true);

            Utils.RemoveAllChildsIn(container);

            all = new List<PropMetaData>();
            OnLoadData();
            foreach (PropMetaData cd in all)
                AddPropMetadata(cd);

            if (Data.Instance.sObjectsData.userMetaData.Count > 0)
                firstLoad = true;

            Events.OnLoading(false);
        }
        public virtual void OnLoadData()
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
        public virtual void OpenWork(string id)
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
            SetTabs();
            Events.OnLoadingParent(transform, LoadingDone);
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
    }

}