using BoardItems;
using System;
using UnityEngine;
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

            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }

        protected override void AddFilmMetadata(FilmDataFabulab fd) {
            if (!fd.isPublic)
                return;
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            go.Init(fd.id, null);
            go.GetComponent<ItemSelectorStory>().SetContent(fd, this, false);
        }

        protected override void OnFilmMetadataUpdated(FilmDataFabulab fd) {
            Debug.Log("% AllStoriesScreen OnFilmMetadataUpdated " + gameObject.name);
            ItemSelectorStory[] itemBtns = worksContainer.GetComponentsInChildren<ItemSelectorStory>();
            ItemSelectorStory btn = Array.Find(itemBtns, x => x.Id == fd.id);
            if (btn != null) {
                if (!fd.isPublic) {
                    Destroy(btn.gameObject);
                } else {
                    btn.Init(fd.GetSprite());
                    btn.UpdatePublicState();
                    btn.transform.SetAsFirstSibling();
                    ResetCache();
                }
            } else {
                if (fd.isPublic) {
                    AddFilmMetadata(fd);
                }
            }
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