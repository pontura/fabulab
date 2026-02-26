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
            Debug.Log("AddSceneObject: " + data);
            if (data is SOAvatarFabulabData)
            {
                Debug.Log("ACA1");
                selected = Instantiate(avatar_to_instantiate);
                AddToContainer(selected, data);
                
                selected.Init(data);
                selectedAvatar = selected;
                inputsManager.ResetAll();
                CharacterManager characterManager = selected.GetComponent<AvatarFabulab>().characterManager;
                Events.LoadBoardItemForStory(characterManager, (data as SOAvatarFabulabData).characterData);
                characterManager.SetColliderActive(false);
            }            
        }

        override protected void RemoveSceneObject()
        {
            base.RemoveSceneObject();
            //UIManager.Instance.mainMenu.CloseAll();
        }
    }
}
