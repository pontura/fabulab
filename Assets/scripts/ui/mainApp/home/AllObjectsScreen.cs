using BoardItems.BoardData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class AllObjectsScreen : MonoBehaviour
    {
        public ItemSelectorBtn workBtn_prefab;
        public Transform container;
        public Transform backgroundsContainer;
        public Transform objectsContainer;
        [SerializeField] TitleScrollView[] titleScrollView;

        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if(isOn)
            {
                Init();
            }
        }
        public void Init()
        {
            Utils.RemoveAllChildsIn(backgroundsContainer);
            Utils.RemoveAllChildsIn(objectsContainer);

            List<PropMetaData> generics = Data.Instance.sObjectsData.GetMetadataByType(SObjectData.types.generic);
            List<PropMetaData> backgrounds  = Data.Instance.sObjectsData.GetMetadataByType(SObjectData.types.background);

            AddTitle(0, "Generic Objects (" + generics.Count + ")");
            foreach (PropMetaData cd in generics)
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, objectsContainer);
                print("go " + go);
                go.Init(cd);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }
            AddTitle(1, "Backgrounds (" + backgrounds.Count + ")");
            foreach (PropMetaData cd in backgrounds)
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, backgroundsContainer);
                print("go " + go);
                go.Init(cd);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }
        }
        void AddTitle(int id, string s)
        {
            TitleScrollView t = titleScrollView[id];
            t.Init(s);
        }
        public void OpenWork(string id)
        {
            UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
        }

        public void New()
        {
            print("New Character");
            UIManager.Instance.NewCharacter();
        }
    }

}