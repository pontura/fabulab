using BoardItems;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllStoriesScreen : MonoBehaviour
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
            foreach(FilmDataFabulab cd in Data.Instance.scenesData.filmsData)
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                print("go " + go);
                go.Init(cd.GetSprite());
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }            
        }        

        public void OpenWork(string id) {
            Data.Instance.scenesData.LoadFilm(id);
            UIManager.Instance.boardUI.SetEditingType(BoardUI.editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            Invoke(nameof(SetStoryEditionState), Time.deltaTime * 2);
        }
        
        void SetStoryEditionState() {
            StoryMakerEvents.EnableStoryEdition(false);
        }
    }

}