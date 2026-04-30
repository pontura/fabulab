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
                go.Init(cd.id, cd.GetSprite());

                if (StoryMakerEvents.isEditing)
                    go.GetComponent<Button>().onClick.AddListener(() => Duplicate(cd.id));
                else
                    go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }

            if (Data.Instance.charactersData.charactersMetaData.Count > 0)
                firstLoad = true;
        }
        public void Duplicate(string soID)
        {
            Events.DuplicateCharacter(soID);
        }                
    }

}