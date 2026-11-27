using BoardItems.BoardData;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AvatarSelectionScreen : ItemSelectionScreen
    {
        public AllCharactersScreen allCharactersScreen;


        protected override void Start()
        {
            base.Start();
            minIndex = 1;
            Cancel();
            Events.UseOthersCharacter += UseOthersCharacter;
            StoryMakerEvents.OnStoryChanged += RefreshUsedCharacters;
        }
        protected override void OnDestroy() {
            base.OnDestroy();
            Events.UseOthersCharacter -= UseOthersCharacter;
            StoryMakerEvents.OnStoryChanged -= RefreshUsedCharacters;
        }

        protected override void LoadNext()
        {
            scrollbar.value = 0;
            AddBtn();
            // The user's own characters first, then the public ones of the community
            var charactersData = Data.Instance.charactersData;
            foreach (SOPartData part in charactersData.userCharacters)
            {
                AddElement(part);
            }
            foreach (CharacterMetaData meta in charactersData.charactersMetaData)
            {
                if (meta.isPublic && !charactersData.IsMyCharacter(meta.id) && charactersData.GetUserCharacter(meta.id) == null)
                    AddFilteredElement(meta.id);
            }
            RefreshUsedCharacters();
            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }

        // A film has each character only once: the ones already in it are greyed out
        void RefreshUsedCharacters() {
            if (ScenesManager.Instance == null)
                return;
            foreach (ItemSelectorBtn btn in worksContainer.GetComponentsInChildren<ItemSelectorBtn>(true)) {
                if (!string.IsNullOrEmpty(btn.Id))
                    btn.SetUsed(ScenesManager.Instance.HasElement(btn.Id, SceneElementType.AVATAR));
            }
        }

        protected override bool IsOwn(string id) {
            return Data.Instance.charactersData.GetUserCharacter(id) != null;
        }

        protected override CharacterMetaData GetMeta(string id) {
            return Data.Instance.charactersData.GetMeta(id);
        }

        public override void OpenWork(string id)
        {
            SOAvatarData data = new SOAvatarData();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
            Scenario.Instance.sceneObjectsManager.AddNewSceneObject(data);
        }
        public override void New()
        {
            Debug.Log($"ReplaceEnabled {ReplaceEnabled}");
            base.New();
            GetComponent<AddNew>().Show(true, Clicked);
        }
        public void Clicked(int id)
        {
            switch (id)
            {
                case 0:
                    UIManager.Instance.CreateSelected(2);
                    FabulabAnalytics.LogEvent(
                        "new_character",
						new System.Collections.Generic.Dictionary<string, object> { { "origin", "story" } }
                    );
                    break;
                case 1:
                    allCharactersScreen.gameObject.SetActive(true);
                    allCharactersScreen.Show(true);
                    break;
            }
        }
        public void Cancel()
        {
            allCharactersScreen.gameObject.SetActive(false);
        }
        // Chosen in the list of all the characters of the community
        void UseOthersCharacter(string id)
        {
            GetComponent<AddNew>().Show(false, null);
            Cancel();
            UseOthersWork(id);
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