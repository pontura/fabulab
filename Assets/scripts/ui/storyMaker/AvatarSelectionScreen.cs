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
        [SerializeField] Scrollbar scrollbar;

        private void Start()
        {
            Cancel();
            Events.DuplicateCharacter += DuplicateCharacter;
        }
        private void OnDestroy()
        {
            Events.DuplicateCharacter -= DuplicateCharacter;
        }
        protected override void LoadNext()
        {
            AddBtn();
            foreach (CharacterData cd in Data.Instance.charactersData.userCharacters)
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                print("go " + go);
                go.Init(cd);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }
            scrollbar.value = 0;
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
                    allCharactersScreen.Init();
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
            CharacterMetaData c =  Data.Instance.charactersData.charactersMetaData.Find(x => x.id == duplicateID);
            if(c != null)
            {
                print("Duplicate open: " + duplicateID);
                Events.OnLoading(false);
                OpenWork(duplicateID);
            }
            else
            {
                if(loops >100) //timeOut:
                    Events.OnLoading(false);
                else
                    Invoke(nameof(LoopTillMetaReady), 0.25f);
            }
        }
    }

}