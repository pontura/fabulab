using BoardItems.Characters;
using UnityEngine;


namespace Yaguar.StoryMaker.Editor
{
    public class SceneObjectsManagerFabulab : SceneObjectsManager
    {
        void Start()
        {
            StoryMakerEvents.AddSceneObject += AddSceneObject;
            StoryMakerEvents.DeleteSceneObject += DeleteSceneObject;
            StoryMakerEvents.RemoveSceneObject += RemoveSceneObject;
            SOAvatarData data = new SOAvatarData();
        }
        void OnDestroy()
        {
            StoryMakerEvents.AddSceneObject -= AddSceneObject;
            StoryMakerEvents.DeleteSceneObject -= DeleteSceneObject;
            StoryMakerEvents.RemoveSceneObject -= RemoveSceneObject;
        }
        
        public override void AddSceneObject(SOData data)
        {
            Debug.Log("AddSceneObject: " + data.id + " " + data);
            if (data is SOAvatarFabulabData)
            {
                Debug.Log("ACA1");
                SceneObject avatar = Instantiate(avatar_to_instantiate);
                AddToContainer(avatar, data);

                avatar.gameObject.name = "Avatar_" + data.id;
                avatar.Init(data);
                inputsManager.ResetAll();
                CharacterManager characterManager = avatar.GetComponent<AvatarFabulab>().characterManager;
                characterManager.characterID = data.id;
                Events.LoadBoardItemForStory(characterManager, data.id);
                selected = avatar;
                selectedAvatar = avatar;
            }

            sceneObjects.Add(selected);
        }

        override protected void RemoveSceneObject()
        {
            base.RemoveSceneObject();
            //UIManager.Instance.mainMenu.CloseAll();
        }
        public override Avatar GetAvatarInSceneById(string id) {
            foreach (SceneObject a in sceneObjects) {
                if (a is AvatarFabulab) {
                    if (a.GetData().id == id)
                        return a as AvatarFabulab;
                }
            }
            return null;
        }
    }
}
