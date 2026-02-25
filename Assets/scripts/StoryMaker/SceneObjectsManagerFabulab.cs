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
                Events.LoadBoardItemForStory(selected.GetComponent<AvatarFabulab>().characterManager, (data as SOAvatarFabulabData).characterData);
            }            
        }

        override protected void RemoveSceneObject()
        {
            base.RemoveSceneObject();
            //UIManager.Instance.mainMenu.CloseAll();
        }
    }
}
