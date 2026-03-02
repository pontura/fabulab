using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class ObjectSelectionScreen : MonoBehaviour
    {
        public CharacterSelectorBtn workBtn_prefab;
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
            print("SO LoadNext " + artID);
            print("SO count:  " + Data.Instance.sObjectsData.data.Count);
            // if (artID >= Data.Instance.characters.Count) return;
            foreach (BoardItems.BoardData.SObjectData cd in Data.Instance.sObjectsData.data)
            {
                CharacterSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                print("go " + go);
                go.Init(cd);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }            
        }        

        public void OpenWork(string id)
        {            
            UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
        }        
    }
}