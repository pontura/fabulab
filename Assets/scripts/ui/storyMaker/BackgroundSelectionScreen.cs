using BoardItems.BoardData;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class BackgroundSelectionScreen : ItemSelectionScreen
    {
        public AllObjectsFullScreenScreen allObjectsScreen;
        [SerializeField] TMPro.TMP_Text introField;

        protected override void Start()
        {
            introField.text = "Empecemos eligiendo un fondo";
            base.Start();
            minIndex = 1;
            Cancel();
            Events.UseOthersSOBG += UseOthersBackground;
            StoryMakerEvents.OnStoryChanged += RefreshIntro;
            RefreshIntro();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Events.UseOthersSOBG -= UseOthersBackground;
            StoryMakerEvents.OnStoryChanged -= RefreshIntro;
        }

        public override void Show(bool isOn)
        {
            base.Show(isOn);
            RefreshIntro();
        }

        // The intro is shown only in this tab, while the scene has no background
        void RefreshIntro()
        {
            SceneData scene = ScenesManager.Instance != null ? ScenesManager.Instance.GetActiveScene() : null;
            bool withoutBackground = scene == null || string.IsNullOrEmpty(scene.bgID);
            introField.gameObject.SetActive(isActive && withoutBackground);
        }

        protected override void LoadNext()
        {
            AddBtn();
            // The user's own backgrounds first, then the public ones of the community
            foreach (SObjectData cd in Data.Instance.sObjectsData.data)
            {
                if (cd.type == SObjectData.types.background) {
                    AddElement(cd);
                }
            }
            foreach (PropMetaData meta in Data.Instance.sObjectsData.metaData)
            {
                if (meta.isPublic && meta.type == SObjectData.types.background && !IsOwn(meta.id)
                    && meta.userID != Data.Instance.userData.userDataInDatabase.uid)
                    AddFilteredElement(meta.id);
            }
            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }        

        protected override bool IsOwn(string id) {
            return Data.Instance.sObjectsData.data.Exists(x => x.id == id);
        }

        protected override CharacterMetaData GetMeta(string id) {
            return Data.Instance.sObjectsData.GetMeta(id);
        }

        public override void OpenWork(string id)
        {
            SOBGData data = new SOBGData();
            data.id = id;
            Scenario.Instance.sceneObjectsManager.AddNewSceneObject(data);
        }
        public override void New()
        {
            GetComponent<AddNew>().Show(true, Clicked);
        }
        public void Clicked(int id)
        {
            switch (id)
            {
                case 0:

                    UIManager.Instance.CreateSelected(4);
                    FabulabAnalytics.LogEvent(
                        "new_object_background",
						new System.Collections.Generic.Dictionary<string, object> { { "origin", "story" } }
                    );
                    break;
                case 1:
                    allObjectsScreen.gameObject.SetActive(true);
                    allObjectsScreen.Init(SObjectData.types.background);
                    break;
            }
        }
        public void Cancel()
        {
            allObjectsScreen.gameObject.SetActive(false);
        }



        // Chosen in the list of all the backgrounds of the community
        void UseOthersBackground(string id)
        {
            GetComponent<AddNew>().Show(false, null);
            Cancel();
            UseOthersWork(id);
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