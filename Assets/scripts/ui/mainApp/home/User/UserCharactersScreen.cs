using BoardItems.BoardData;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class UserCharactersScreen : ThumbsScreen
    {
        protected override void Start() {
            base.Start();
            Events.OnCharacterMetadataUpdated += OnCharacterMetadataUpdated;
            Events.OnCharacterMetadataAdded += OnCharacterMetadataAdded;
            Events.OnCharacterMetadataRemoved += OnCharacterMetadataRemoved;
        }
        protected virtual void OnDestroy() {
            Events.OnCharacterMetadataUpdated -= OnCharacterMetadataUpdated;
            Events.OnCharacterMetadataAdded -= OnCharacterMetadataAdded;
            Events.OnCharacterMetadataRemoved -= OnCharacterMetadataRemoved;
        }
        
        void OnCharacterMetadataAdded(CharacterMetaData fd) {
            AddCharacterMetadata(fd);
            worksContainer.GetChild(worksContainer.childCount - 1).SetAsFirstSibling();
            if (firstImageCache) {
                ResetCache();
            }
        }

        protected  virtual void AddCharacterMetadata(CharacterMetaData fd) {
            Debug.Log("% AddCharacterMetadata");
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);

            go.Init(fd, MetadataTypes.characters, OpenWork, true);

            //pelu     go.Init(fd, MetadataTypes.characters);
            //pelu     go.GetComponent<Button>().onClick.AddListener(() => OpenWork(fd.id));

        }

        protected virtual void OnCharacterMetadataUpdated(CharacterMetaData fd) {
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                btn.Init(fd.GetSprite());
                btn.UpdatePublicState();
                btn.transform.SetAsFirstSibling();
                ResetCache();
            } else {
                OnCharacterMetadataAdded(fd);
            }
        }

        void OnCharacterMetadataRemoved(string id) {
            ItemSelectorBtn[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorBtn>();
            ItemSelectorBtn btn = Array.Find(itemBtns, x => x.Id == id);
            if (btn != null) {
                Destroy(btn.gameObject);
            }
        }

        /*public void Init()
        {
            Events.OnLoadingParent(transform, LoadingDone);
        }
        void LoadingDone()
        {
            Events.OnLoading(true);

            foreach (Transform child in worksContainer)
            {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }

            LoadNext();
        }*/

        protected override void LoadNext()
        {
            foreach(CharacterMetaData chmd in Data.Instance.charactersData.userCharactersMetaData)
            {
                AddCharacterMetadata(chmd);                
            }

            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }

        public override void OpenWork(string id) { 
            UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);    
        }

        public virtual void New()
        {
            print("New Character");
            UIManager.Instance.NewCharacter();
        }

        protected override void LoadImage(int index, ItemSelectorBtn isb) {
            CharacterMetaData chMD = Data.Instance.charactersData.charactersMetaData.Find(x => x.id == isb.Id);
            if (chMD != null) {
                downloading.Add(index);
                Debug.Log($"$ DownloadTexture index: {index} Id: {chMD.id}");
                Data.Instance.cacheData.LoadImage(BoardItems.BoardData.MetadataTypes.characters.ToString(), chMD.id, (tex) => {
                    downloading.Remove(index);
                    isb.SetSprite(tex);
                    imageCache[index] = tex;
                    Debug.Log($"ImageCache: {imageCache.Count}");
                    if (!firstImageCache && imageCache.Count >= (cacheSize - cacheExtraItemsCount)) {
                        firstImageCache = true;
                        Events.OnLoading(false);
                    }
                }, chMD.timestamp, chMD.userID);
            } else {
                Debug.LogError("Couldn´t find Film Metadata with ID " + isb.Id);
            }
        }
    }

}