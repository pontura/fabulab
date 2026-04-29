using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class BackgroundSelectionScreen : ItemSelectionScreen
    {
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
            UIManager.Instance.NewObject(SObjectData.types.background);
        }
    }
}