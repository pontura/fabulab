using BoardItems;
using BoardItems.BoardData;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class UserCharactersScreen : MonoBehaviour
    {
        public ItemSelectorBtn workBtn_prefab;
        public Transform worksContainer;

        protected int artID = 0;
        protected bool firstLoad;

        protected virtual void Start() {
            Events.OnCharacterMetadataUpdated += OnCharacterMetadataUpdated;
            Events.OnCharacterMetadataAdded += OnCharacterMetadataAdded;
            Events.OnCharacterMetadataRemoved += OnCharacterMetadataRemoved;
        }
        protected virtual void OnDestroy() {
            Events.OnCharacterMetadataUpdated += OnCharacterMetadataUpdated;
            Events.OnCharacterMetadataAdded -= OnCharacterMetadataAdded;
            Events.OnCharacterMetadataRemoved += OnCharacterMetadataRemoved;
        }
        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn && !firstLoad) {
                Init();
            }
        }

        void OnCharacterMetadataAdded(CharacterMetaData fd) {
            AddCharacterMetadata(fd);
            worksContainer.GetChild(worksContainer.childCount - 1).SetAsFirstSibling();
        }

        void AddCharacterMetadata(CharacterMetaData fd) {
            Debug.Log("% AddCharacterMetadata");
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            go.Init(fd);
            go.GetComponent<Button>().onClick.AddListener(() => OpenWork(fd.id));
        }

        void OnCharacterMetadataUpdated(CharacterMetaData fd) {
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                btn.Init(fd.GetSprite());
                btn.transform.SetAsFirstSibling();
            }
        }

        void OnCharacterMetadataRemoved(string id) {
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == id);
            if (btn != null) {
                Destroy(btn.gameObject);
            }
        }

        public void Init()
        {
            Events.OnLoading(true, LoadingType.Home);
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
            foreach(CharacterMetaData chmd in Data.Instance.charactersData.userCharactersMetaData)
            {
                AddCharacterMetadata(chmd);                
            }
            if (Data.Instance.charactersData.userCharactersMetaData.Count > 0)
                firstLoad = true;

            Events.OnLoading(false, UI.LoadingType.Home);
        }

        public virtual void OpenWork(string id) { 
            UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);    
        }

        public virtual void New()
        {
            print("New Character");
            UIManager.Instance.NewCharacter();
        }
    }

}