using BoardItems.BoardData;
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
            AddTitle(0, "Objects");
            foreach (BoardItems.BoardData.SOData cd in Data.Instance.sObjectsData.data)
            {
                CharacterSelectorBtn go = Instantiate(workBtn_prefab, objectsContainer);
                print("go " + go);
                go.Init(cd);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }
            AddTitle(1, "Backgrounds");
            foreach (BoardItems.BoardData.SOData cd in Data.Instance.sObjectsData.data)
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