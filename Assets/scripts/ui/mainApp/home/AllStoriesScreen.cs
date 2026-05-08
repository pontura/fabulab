using BoardItems;
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
                AddFilmMetadata(cd);
            }

            if (Data.Instance.scenesData.filmsData.Count > 0) {
                firstLoad = true;
                Events.OnLoading(false);
            }
        }

        protected override void AddFilmMetadata(FilmDataFabulab fd) {
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            go.Init(fd.id, fd.GetSprite());
            go.GetComponent<ItemSelectorStory>().SetContent(fd, this, false);
        }

        string id;
        public override void OpenWork(string id) {
            this.id = id;
            Events.OnLoadingParent(null, LoadingDone);
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
            StoryMakerEvents.EnableInputManager(false);
        }

    }

}