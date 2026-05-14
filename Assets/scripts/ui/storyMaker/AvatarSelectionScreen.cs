using BoardItems.BoardData;
using System;
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
        void DuplicateCharacter(string id)
        {
            Events.OnLoading(true);
            Data.Instance.charactersData.Duplicate(id, OnDuplicated);
            GetComponent<AddNew>().Show(false, null);
          //  UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);
            Cancel();
        }

        private void OnDuplicated(bool arg1, string arg2)
        {
            Events.OnLoading(false);
            throw new NotImplementedException();
        }
    }

}