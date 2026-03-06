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
            if (data is SOAvatarFabulabData avatarData)
            {
                Debug.Log("ACA1");
                SceneObject avatar = Instantiate(avatar_to_instantiate);
                AddToContainer(avatar, avatarData);

                avatar.gameObject.name = "Avatar_" + avatarData.id;
                avatar.Init(avatarData);
                inputsManager.ResetAll();
                CharacterManager characterManager = avatar.GetComponent<AvatarFabulab>().characterManager;
                characterManager.characterID = avatarData.id;
                Events.LoadBoardItemForStory(characterManager, avatarData.id);
                selected = avatar;
                selectedAvatar = avatar;

                if (avatarData.anim == CharacterAnims.anims.edit)
                    avatarData.anim = CharacterAnims.anims.idle;
                Events.OnCharacterAnim(avatarData.id,avatarData.anim);
                Events.OnCharacterExpression(avatarData.id, avatarData.emoji);
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
