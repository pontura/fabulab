using BoardItems.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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
    }
}
