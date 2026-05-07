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
                go.GetComponent<ItemSelectorStory>().SetContent(cd);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }

            if (Data.Instance.scenesData.filmsData.Count > 0)
                firstLoad = true;

            Events.OnLoading(false);
        }
        string id;
        public override void OpenWork(string id) {
            this.id = id;
            Events.OnLoadingParent(transform, LoadingDone);
        }
        void LoadingDone()
        {
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