using BoardItems.BoardData;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class AvatarSelectionScreen : ItemSelectionScreen
    {
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
        }

        public override void OpenWork(string id) {
            SOAvatarFabulabData data = new SOAvatarFabulabData();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
            StoryMakerEvents.AddSceneObject(data);
        }
        public override void New() { 
        }
    }

}