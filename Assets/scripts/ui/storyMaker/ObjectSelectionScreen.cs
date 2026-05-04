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
        }
        private void OnDestroy()
        {
            Events.DuplicateSO -= DuplicateSO;
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
        void DuplicateSO(string id)
        {
            GetComponent<AddNew>().Show(false, null);
            UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
            Cancel();
        }
        float maxZ = 50;
        float minZ = -50;
        float offsetZ = 0.1f;
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
        void SetFrontBack()
        {
            if (Scenario.Instance.GetComponent<SceneObjectsManagerFabulab>().selected == null) return;
            force_z = Scenario.Instance.GetComponent<SceneObjectsManagerFabulab>().selected.GetData().force_z;
            if(force_z == 0)
            {
                sendFront.SetIsOnWithoutNotify(false);
                sendBack.SetIsOnWithoutNotify(false);
            }else if (force_z > 0)
            {
                sendFront.SetIsOnWithoutNotify(true);
                sendBack.SetIsOnWithoutNotify(false);
            }
            else if (force_z < 0)
            {
                sendFront.SetIsOnWithoutNotify(false);
                sendBack.SetIsOnWithoutNotify(true);
            }
        }
    }
}