using BoardItems.BoardData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class UserObjectsScreen : MonoBehaviour
    {
        public CharacterSelectorBtn workBtn_prefab;
        public Transform container;
        public Transform backgroundsContainer;
        public Transform objectsContainer;
        [SerializeField] TitleScrollView[] titleScrollView;

        int artID = 0;
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
            artID = 0;
            LoadNext();
        }        
        void LoadNext()
        {
            Utils.RemoveAllChildsIn(backgroundsContainer);
            Utils.RemoveAllChildsIn(objectsContainer);

            List<SObjectData> generics = Data.Instance.sObjectsData.GetDataByType(SObjectData.types.generic);
            List<SObjectData> backgrounds  = Data.Instance.sObjectsData.GetDataByType(SObjectData.types.background);

            AddTitle(0, "Generic Objects (" + generics.Count + ")");
            foreach (SObjectData cd in generics)
            {
                CharacterSelectorBtn go = Instantiate(workBtn_prefab, objectsContainer);
                print("go " + go);
                go.Init(cd);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }
            AddTitle(1, "Backgrounds (" + backgrounds.Count + ")");
            foreach (SObjectData cd in backgrounds)
            {
                CharacterSelectorBtn go = Instantiate(workBtn_prefab, backgroundsContainer);
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