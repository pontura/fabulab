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
        public Transform container;
        public Transform backgroundsContainer;
        public Transform objectsContainer;
        [SerializeField] protected TitleScrollView[] titleScrollView;

        protected bool firstLoad;

        protected virtual void Start() {
            Events.OnPropMetadataUpdated += OnPropMetadataUpdated;
            Events.OnPropMetadataAdded += OnPropMetadataAdded;
            Events.OnPropMetadataRemoved += OnPropMetadataRemoved;
        }
        protected virtual void OnDestroy() {
            Events.OnPropMetadataUpdated += OnPropMetadataUpdated;
            Events.OnPropMetadataAdded -= OnPropMetadataAdded;
            Events.OnPropMetadataRemoved += OnPropMetadataRemoved;
        }

        void OnPropMetadataAdded(PropMetaData fd) {
            AddPropMetadata(fd);
            Transform t = objectsContainer;
            if (fd.type == SObjectData.types.background)
                t = backgroundsContainer;
            t.GetChild(t.childCount - 1).SetAsFirstSibling();
        }

        protected void AddPropMetadata(PropMetaData fd) {
            Debug.Log("% AddCharacterMetadata");
            Transform t = objectsContainer;
            if (fd.type == SObjectData.types.background)
                t = backgroundsContainer;
            ItemSelectorBtn go = Instantiate(workBtn_prefab, t);
            go.Init(fd);
            go.GetComponent<Button>().onClick.AddListener(() => OpenWork(fd.id));
        }

        void OnPropMetadataUpdated(PropMetaData fd) {
            Transform t = objectsContainer;
            if (fd.type == SObjectData.types.background)
                t = backgroundsContainer;
            ItemSelectorBtn[] itemBtns = t.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                btn.Init(fd);
                btn.transform.SetAsFirstSibling();
            }
        }

        void OnPropMetadataRemoved(PropMetaData fd) {
            Transform t = objectsContainer;
            if (fd.type == SObjectData.types.background)
                t = backgroundsContainer;
            ItemSelectorBtn[] itemBtns = t.GetComponentsInChildren<ItemSelectorBtn>();
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
            Events.OnLoading(true,LoadingType.Home);

            Utils.RemoveAllChildsIn(backgroundsContainer);
            Utils.RemoveAllChildsIn(objectsContainer);

            List<PropMetaData> generics = Data.Instance.sObjectsData.GetUserMetadataByType(SObjectData.types.generic);
            List<PropMetaData> backgrounds  = Data.Instance.sObjectsData.GetUserMetadataByType(SObjectData.types.background);

            AddTitle(0, "Generic Objects (" + generics.Count + ")");
            foreach (PropMetaData cd in generics)
            {
                AddPropMetadata(cd);
            }
            AddTitle(1, "Backgrounds (" + backgrounds.Count + ")");
            foreach (PropMetaData cd in backgrounds)
            {
                AddPropMetadata(cd);
            }

            if (Data.Instance.sObjectsData.userMetaData.Count > 0)
                firstLoad = true;

            Events.OnLoading(false, UI.LoadingType.Home);
        }
        protected virtual void AddTitle(int id, string s)
        {
            TitleScrollView t = titleScrollView[id];
            t.Init(s);
        }
        public virtual void OpenWork(string id)
        {
            UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
        }

        public virtual void New()
        {
            print("New Object");
            UIManager.Instance.NewCharacter();
        }
    }

}