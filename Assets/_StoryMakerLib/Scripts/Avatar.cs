using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace Yaguar.StoryMaker.Editor
{
    public class Avatar : SceneObject
    {

        protected GameObject asset;

        private void Start()
        {
            StoryMakerEvents.SetAvatarData += SetData;
        }
        private void OnDestroy()
        {
            StoryMakerEvents.SetAvatarData -= SetData;
        }        

        public virtual void Run()
        {

        }

        public virtual void Walk()
        {

        }




    }
}
