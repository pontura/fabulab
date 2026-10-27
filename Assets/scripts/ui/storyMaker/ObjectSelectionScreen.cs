using BoardItems.BoardData;
using Firebase.Analytics;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class ObjectSelectionScreen : ItemSelectionScreen
    {
        public AllObjectsFullScreenScreen allObjectsScreen;

        [SerializeField] Toggle sendFront;
        [SerializeField] Toggle sendBack;


        protected override void Start() {
            base.Start();
            minIndex = 1;
            Cancel();
            Events.DuplicateSO += DuplicateSO;
            StoryMakerEvents.ShowSoButtons += ShowSoButtons;
        }
        public override void Show(bool isOn)
        {
            base.Show(isOn);
            if(!isOn)
                GetComponent<StoryEditorScreen>().ShowToolsInStories(isOn);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Events.DuplicateSO -= DuplicateSO;
            StoryMakerEvents.ShowSoButtons -= ShowSoButtons;
        }
        protected override void LoadNext()
        {
            AddBtn();
            // The user's own objects first, then the public ones of the community
            foreach (SObjectData cd in Data.Instance.sObjectsData.data)
            {
                if (cd.type != SObjectData.types.background) {
                    AddElement(cd);
                }
            }
            foreach (PropMetaData meta in Data.Instance.sObjectsData.metaData)
            {
                if (meta.isPublic && meta.type != SObjectData.types.background && !IsOwn(meta.id)
                    && meta.userID != Data.Instance.userData.userDataInDatabase.uid)
                    AddFilteredElement(meta.id);
            }
            SetFrontBack();
            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }        

        protected override bool IsOwn(string id) {
            return Data.Instance.sObjectsData.data.Exists(x => x.id == id);
        }

        protected override void DuplicateAndSelect(string id) {
            DuplicateSO(id);
        }

        protected override CharacterMetaData GetMeta(string id) {
            return Data.Instance.sObjectsData.GetMeta(id);
        }

        public override void OpenWork(string id)
        {
            SOPropData data = new SOPropData();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
            Scenario.Instance.sceneObjectsManager.AddNewSceneObject(data);
        }
        public override void New()
        {
            base.New();
            GetComponent<AddNew>().Show(true, Clicked);
        }
        public void Clicked(int id)
        {
            switch (id)
            {
                case 0:
                    UIManager.Instance.CreateSelected(3);
                    FabulabAnalytics.LogEvent(
                        "new_object_generic",
						new System.Collections.Generic.Dictionary<string, object> { { "origin", "story" } }
                    );
                    break;
                case 1:
                    allObjectsScreen.gameObject.SetActive(true);
                    allObjectsScreen.Init(SObjectData.types.generic);
                    break;
            }
        }
        public void Cancel()
        {
            allObjectsScreen.gameObject.SetActive(false);
        }
       
        float maxZ = 50;
        float minZ = -50;
        float offsetZ = 1;
        float force_z;
        public void SetFront(bool front)
        {
            if(front)
            {
                if (force_z <= 0)
                {
                    maxZ += offsetZ;
                    Scenario.Instance.GetComponent<SceneObjectsManager>().ForceZ(maxZ);
                    force_z = maxZ;
                }
                else
                {
                    Scenario.Instance.GetComponent<SceneObjectsManager>().ForceZ(0);
                    force_z = 0;
                }
            }
            else
            {
                if (force_z >= 0)
                {
                    minZ -= offsetZ;
                    Scenario.Instance.GetComponent<SceneObjectsManager>().ForceZ(minZ);
                    force_z = minZ;
                }
                else
                {
                    Scenario.Instance.GetComponent<SceneObjectsManager>().ForceZ(0);
                    force_z = 0;
                }
            }
            SetFrontBack();
        }
        void ShowSoButtons(Vector3 pos, SOData data)
        {
            if(!isActive) return;
            SetFrontBack();
        }
        void SetFrontBack()
        {
            if (Scenario.Instance.GetComponent<SceneObjectsManager>().selected == null) return;
            force_z = Scenario.Instance.GetComponent<SceneObjectsManager>().selected.GetData().force_z;
            print("SetFrontBack force_z " + force_z);
            if(force_z == 0)
            {
                sendFront.SetIsOnWithoutNotify(false);
                sendBack.SetIsOnWithoutNotify(false);
            }else if (force_z < 0)
            {
                sendFront.SetIsOnWithoutNotify(true);
                sendBack.SetIsOnWithoutNotify(false);
            }
            else if (force_z > 0)
            {
                sendFront.SetIsOnWithoutNotify(false);
                sendBack.SetIsOnWithoutNotify(true);
            }
        }



        string duplicateID;
        void DuplicateSO(string duplicateID)
        {
            print("DuplicateSO open: " + duplicateID);
            this.duplicateID = duplicateID;
            Events.OnLoadingParent(null, DuplicateAction);
        }
        void DuplicateAction()
        {
            Events.OnLoading(true);
            Data.Instance.sObjectsData.Duplicate(duplicateID, OnDuplicated);
            GetComponent<AddNew>().Show(false, null);
            //  UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);
            Cancel();
        }
        float loops;
        private void OnDuplicated(bool success, string duplicateID)
        {
            this.duplicateID = duplicateID;
            if (success) {
                loops = 0;
                LoopTillMetaReady();
            } else {
                Events.OnLoading(false);
                StoryMakerEvents.SetEditing(false);
            }
		}
        private void LoopTillMetaReady()
        {
            loops++;
            SOPartData part = Data.Instance.sObjectsData.data.Find(x => x.id == duplicateID);
            if (part != null)
            {
                AddOwnElement(part.id);
                Events.OnLoading(false);
                SelectWork(duplicateID);
            }
            else
            {
                if (loops > 100) //timeOut:
                    Events.OnLoading(false);
                else
                    Invoke(nameof(LoopTillMetaReady), 0.25f);
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
                Debug.LogError("Couldn´t find Object Metadata with ID " + isb.Id);
            }
        }
    }
}