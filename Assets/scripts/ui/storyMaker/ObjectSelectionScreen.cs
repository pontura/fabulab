using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class ObjectSelectionScreen : ItemSelectionScreen
    {
        public AllObjectsFullScreenScreen allObjectsScreen;
        [SerializeField] Scrollbar scrollbar;

        [SerializeField] Toggle sendFront;
        [SerializeField] Toggle sendBack;

        private void Start()
        {
            Cancel();
            Events.DuplicateSO += DuplicateSO;
            StoryMakerEvents.ShowSoButtons += ShowSoButtons;
        }
        private void OnDestroy()
        {
            Events.DuplicateSO -= DuplicateSO;
            StoryMakerEvents.ShowSoButtons -= ShowSoButtons;
        }
        protected override void LoadNext()
        {
            AddBtn();
            foreach (SObjectData cd in Data.Instance.sObjectsData.data)
            {
                if (cd.type != SObjectData.types.background) {
                    ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                    print("go " + go);
                    go.Init(cd);
                    go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
                }
            }
            scrollbar.value = 0;
            SetFrontBack();
        }        

        public override void OpenWork(string id)
        {
            SODataFabulab data = new SODataFabulab();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
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
                    UIManager.Instance.NewObject(SObjectData.types.generic);
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
                    Scenario.Instance.GetComponent<SceneObjectsManagerFabulab>().ForceZ(maxZ);
                    force_z = maxZ;
                }
                else
                {
                    Scenario.Instance.GetComponent<SceneObjectsManagerFabulab>().ForceZ(0);
                    force_z = 0;
                }
            }
            else
            {
                if (force_z >= 0)
                {
                    minZ -= offsetZ;
                    Scenario.Instance.GetComponent<SceneObjectsManagerFabulab>().ForceZ(minZ);
                    force_z = minZ;
                }
                else
                {
                    Scenario.Instance.GetComponent<SceneObjectsManagerFabulab>().ForceZ(0);
                    force_z = 0;
                }
            }
            SetFrontBack();
        }
        void ShowSoButtons(Vector3 pos, SOData data)
        {
            SetFrontBack();
        }
        void SetFrontBack()
        {
            if (Scenario.Instance.GetComponent<SceneObjectsManagerFabulab>().selected == null) return;
            force_z = Scenario.Instance.GetComponent<SceneObjectsManagerFabulab>().selected.GetData().force_z;
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