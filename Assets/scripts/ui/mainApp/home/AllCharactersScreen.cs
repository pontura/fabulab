using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AllCharactersScreen : UserCharactersScreen
    {

        protected override void LoadNext()
        {
            foreach(CharacterMetaData cd in Data.Instance.charactersData.charactersMetaData)
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                print("go " + go);
                go.Init(cd);

                if (StoryMakerEvents.isEditing)
                    go.GetComponent<Button>().onClick.AddListener(() => Duplicate(cd.id));
                else
                    go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }

            if (Data.Instance.charactersData.charactersMetaData.Count > 0)
                firstLoad = true;

            Events.OnLoading(false, UI.LoadingType.Home);
        }
        public void Duplicate(string soID)
        {
            Events.DuplicateCharacter(soID);
        }                
    }

}