using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllCharactersScreen : MonoBehaviour
    {
        public ItemSelectorBtn workBtn_prefab;
        public Transform worksContainer;

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

            foreach (Transform child in worksContainer)
            {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }

            LoadNext();
        }
        
        void LoadNext()
        {
            foreach(CharacterMetaData cd in Data.Instance.charactersData.charactersMetaData)
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                print("go " + go);
                go.Init(cd);

                if(StoryMakerEvents.isEditing)
                    go.GetComponent<Button>().onClick.AddListener(() => Duplicate(cd.id));
                else
                    go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }
        }
        public void Duplicate(string soID)
        {
            Events.DuplicateCharacter(soID);
        }

        public void OpenWork(string id) { 
            UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);    
        }

        public void New()
        {
            print("New Character");
            UIManager.Instance.NewCharacter();
        }
    }

}