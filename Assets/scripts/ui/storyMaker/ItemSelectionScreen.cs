using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class ItemSelectionScreen : MonoBehaviour
    {
        [SerializeField] Button addBtn;
        public ItemSelectorBtn workBtn_prefab;
        public Transform worksContainer;

        public void AddBtn()
        {
            Button go = Instantiate(addBtn, worksContainer);
            go.GetComponent<Button>().onClick.AddListener(() => New());
        }
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

            foreach (Transform child in worksContainer)
            {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }
            LoadNext();
        }
        
        protected virtual void LoadNext()
        {
            foreach(CharacterData cd in Data.Instance.charactersData.userCharacters)
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                print("go " + go);
                go.Init(cd);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }           
        }
        public virtual void New(){ }
        public virtual void OpenWork(string id) {
            SOAvatarFabulabData data = new SOAvatarFabulabData();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
            StoryMakerEvents.SetSceneObject(data);
            //UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);    
        }        
    }

}