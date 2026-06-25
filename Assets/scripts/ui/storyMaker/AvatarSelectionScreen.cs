using BoardItems.BoardData;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AvatarSelectionScreen : ItemSelectionScreen
    {
        public AllCharactersScreen allCharactersScreen;
        

        protected override void Start()
        {
            base.Start();
            minIndex = 1;
            Cancel();
            Events.DuplicateCharacter += DuplicateCharacter;
        }
        protected virtual void OnDestroy() {
            Events.DuplicateCharacter -= DuplicateCharacter;
        }

        protected override void LoadNext()
        {
            scrollbar.value = 0;
            AddBtn();
            foreach (SOPartData part in Data.Instance.charactersData.userCharacters)
            {
                AddElement(part);
            }            
            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }

        public override void OpenWork(string id)
        {
            print("OpenWork " + id);
            SOAvatarFabulabData data = new SOAvatarFabulabData();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
            StoryMakerEvents.AddSceneObject(data);
        }
        public override void New()
        {
            Debug.Log($"ReplaceEnabled {ReplaceEnabled}");
            base.New();
            GetComponent<AddNew>().Show(true, Clicked);
        }
        public void Clicked(int id)
        {
            switch (id)
            {
                case 0:
                    UIManager.Instance.NewCharacter();
                    break;
                case 1:
                    allCharactersScreen.gameObject.SetActive(true);
                    allCharactersScreen.Show(true);
                    break;
            }
        }
        public void Cancel()
        {
            allCharactersScreen.gameObject.SetActive(false);
        }
        string duplicateID;
        void DuplicateCharacter(string duplicateID)
        {
            this.duplicateID = duplicateID;
            Events.OnLoadingParent(null, DuplicateAction);          
        }
        void DuplicateAction()
        {
            Events.OnLoading(true);
            Data.Instance.charactersData.Duplicate(duplicateID, OnDuplicated);
            GetComponent<AddNew>().Show(false, null);
            //  UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);
            Cancel();
        }
        float loops;
        private void OnDuplicated(bool success, string duplicateID)
        {
            this.duplicateID = duplicateID;
            if (success)
            {
                loops = 0;
                LoopTillMetaReady();
            }
            else
                Events.OnLoading(false);
        }
        private void LoopTillMetaReady()
        {
            loops++;
            SOPartData part =  Data.Instance.charactersData.userCharacters.Find(x => x.id == duplicateID);
            if(part != null)
            {
                AddElement(part);
                print("Duplicate open: " + duplicateID);
                Events.OnLoading(false);                
                SelectWork(duplicateID);
            }
            else
            {
                if(loops >100) //timeOut:
                    Events.OnLoading(false);
                else
                    Invoke(nameof(LoopTillMetaReady), 0.25f);
            }
        }

        protected override void LoadImage(int index, ItemSelectorBtn isb) {
            CharacterMetaData chMD = Data.Instance.charactersData.charactersMetaData.Find(x => x.id == isb.Id);
            if (chMD != null) {
                downloading.Add(index);
                Debug.Log($"$ DownloadTexture index: {index} Id: {chMD.id}");
                Data.Instance.cacheData.LoadImage(BoardItems.BoardData.MetadataTypes.characters.ToString(), chMD.id, (tex) => {
                    downloading.Remove(index);
                    isb.SetSprite(tex);
                    imageCache[index] = tex;
                    Debug.Log($"ImageCache: {imageCache.Count}");
                    if (!firstImageCache && imageCache.Count >= (cacheSize - cacheExtraItemsCount)) {
                        firstImageCache = true;
                        Events.OnLoading(false);
                    }
                }, chMD.timestamp, chMD.userID);
            } else {
                Debug.LogError("Couldn´t find Film Metadata with ID " + isb.Id);
            }
        }
    }

}