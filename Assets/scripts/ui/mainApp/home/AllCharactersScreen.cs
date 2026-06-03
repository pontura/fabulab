using BoardItems.BoardData;
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

                if (StoryMakerEvents.isEditing)
                    go.Init(cd, Duplicate);
                else
                    go.Init(cd, OpenWork);
            }

            OnLoadedDone();
        }
        public void Duplicate(string soID)
        {
            Events.DuplicateCharacter(soID);
        }                
    }

}