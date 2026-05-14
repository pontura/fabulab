using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class BackgroundSelectionScreen : ItemSelectionScreen
    {
        public AllObjectsFullScreenScreen allObjectsScreen;
        [SerializeField] Scrollbar scrollbar;

        private void Start()
        {
            Cancel();
            Events.DuplicateSOBG += DuplicateSOBG;
        }
        private void OnDestroy()
        {
            Events.DuplicateSOBG -= DuplicateSOBG;
        }

        protected override void LoadNext()
        {
            AddBtn();
            foreach (SObjectData cd in Data.Instance.sObjectsData.data)
            {
                Debug.Log("== Type: " + cd.type);
                if (cd.type == SObjectData.types.background) {
                    ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                    print("BackgroundSelectionScreen " + go + " type: " + cd.type + " id: " + cd.id);
                    go.Init(cd);
                    go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
                }
            }
            scrollbar.value = 0;
        }        

        public override void OpenWork(string id)
        {
            SOBGData data = new SOBGData();
            data.id = id;
            StoryMakerEvents.AddSceneObject(data);
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

                    UIManager.Instance.NewObject(SObjectData.types.background);
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



        string duplicateID;
        void DuplicateSOBG(string duplicateID)
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
            if (success)
            {
                loops = 0;
                LoopTillMetaReady();
            }
            else
                Events.OnLoading(false);
        }
        private void LoopTillMetaReady()
        {
            loops++;
            CharacterMetaData c = Data.Instance.sObjectsData.metaData.Find(x => x.id == duplicateID);
            if (c != null)
            {
                print("Duplicate open: " + duplicateID);
                Events.OnLoading(false);
                OpenWork(duplicateID);
            }
            else
            {
                if (loops > 100) //timeOut:
                    Events.OnLoading(false);
                else
                    Invoke(nameof(LoopTillMetaReady), 0.25f);
            }
        }
    }
}