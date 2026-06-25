using BoardItems.BoardData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class ItemSelectionScreen : ThumbsScreen
    {
        [SerializeField] Button addBtn;
        [SerializeField] protected Scrollbar scrollbar;

        [field:SerializeField] public bool ReplaceEnabled {  get; set; }

        public void AddBtn()
        {
            Button go = Instantiate(addBtn, worksContainer);
            go.GetComponent<Button>().onClick.AddListener(() => New());            
        }

        public override void Show(bool isOn) {
            if (isActive == isOn)
                return;
            isActive = isOn;
            if (isOn) {
                scrollRect.StopMovement();
                scrollbar.value = 0;
                firstImageCache = false;
                imageCache.Clear();
                downloading.Clear();
                Init();
            }
        }

        protected override void Init() {
           // Events.OnLoadingParent(transform, LoadNext);
            foreach (Transform child in worksContainer) {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }
           LoadNext();
        }

        protected virtual void AddElement(SOPartData part) {
            ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
            print("go " + go);
            go.Init(part, SelectWork);
        }

        protected override void LoadNext()
        {
            foreach(SOPartData part in Data.Instance.charactersData.userCharacters)
            {
                AddElement(part);
            }

            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }
        public virtual void New(){
            if (ReplaceEnabled)
                GetComponent<AddNew>().SetActiveButton(0, false);
            else
                GetComponent<AddNew>().SetActiveButton(0, true);
        }

        protected virtual void SelectWork(string id){
            if (ReplaceEnabled) {
                StoryMakerEvents.ReplaceSceneObject(id);
                ReplaceEnabled = false;
            } else
                OpenWork(id);
        }
        
        public override void OpenWork(string id) {
            SOAvatarFabulabData data = new SOAvatarFabulabData();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
            StoryMakerEvents.SetSceneObject(data);
            //UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);    
        }   

        protected override void LoadImage(int index, ItemSelectorBtn isb) {
            /*FilmDataFabulab fd = Data.Instance.scenesData.filmsData.Find(x => x.id == isb.Id);
            if (fd != null) {
                downloading.Add(index);
                Debug.Log($"$ DownloadTexture index: {index} Id: {fd.id}");
                Data.Instance.cacheData.LoadImage(BoardItems.BoardData.MetadataTypes.stories.ToString(), fd.id, (tex) => {
                    downloading.Remove(index);
                    isb.SetSprite(tex);
                    imageCache[index] = tex;
                    Debug.Log($"ImageCache: {imageCache.Count}");
                    if (!firstImageCache && imageCache.Count >= (visibleRows * itemsPerRows)) {
                        firstImageCache = true;
                        Events.OnLoading(false);
                    }
                }, fd.timestamp, fd.userID);
            } else {
                Debug.LogError("Couldn´t find Film Metadata with ID " + isb.Id);
            }*/
        }
    }

}