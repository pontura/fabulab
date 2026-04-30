using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class ObjectSelectionScreen : ItemSelectionScreen
    {
        public AllObjectsScreen allObjectsScreen;
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
                    allObjectsScreen.Init();
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
    }
}