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

                if (StoryMakerEvents.isEditing)
                     go.Init(cd, MetadataTypes.characters, Duplicate);
                else
                    go.Init(cd, MetadataTypes.characters, OpenWork);
            }

            Invoke(nameof(OnLoadedDone), Time.deltaTime * 3);
        }
        public void Duplicate(string soID)
        {
            Events.DuplicateCharacter(soID);
        }                
    }

}