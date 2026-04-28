using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class ObjectSelectionScreen : ItemSelectionScreen
    {
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
    }
}