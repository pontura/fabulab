using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class ObjectSelectionScreen : MonoBehaviour
    {
        public ItemSelectorBtn workBtn_prefab;
        public Transform worksContainer;

        int artID = 0;        

        public void Init()
        {
            artID = 0;

            foreach (Transform child in worksContainer)
            {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }

            LoadNext();
        }
        
        void LoadNext()
        {
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

        public void OpenWork(string id)
        {
            SODataFabulab data = new SODataFabulab();
            data.id = id;
            StoryMakerEvents.AddSceneObject(data);
        }        
    }
}