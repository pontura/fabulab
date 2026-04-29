using BoardItems;
using System;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllStoriesScreen : UserStoriesScreen
    {        
        
        protected override void LoadNext()
        {
            Debug.Log("% AllStoriesScreen LoadNext");
            foreach(FilmDataFabulab cd in Data.Instance.scenesData.filmsData)
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                print("go " + go);
                go.Init(cd.id, cd.GetSprite());
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }

            if (Data.Instance.scenesData.filmsData.Count > 0)
                firstLoad = true;
        }        

        public override void OpenWork(string id) {
            Events.OnLoading(true);
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