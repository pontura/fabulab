using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace Yaguar.StoryMaker.Editor
{
    public class Avatar : SceneObject
    {

        protected GameObject asset;
        protected Animator anim;
        [SerializeField] protected Animator headAnim;

        private void Start()
        {
            StoryMakerEvents.SetAvatarData += SetData;
        }
        private void OnDestroy()
        {
            StoryMakerEvents.SetAvatarData -= SetData;
        }

        public Animator GetAnimator()
        {
            return anim;
        }
        public Animator GetHeadAnimator()
        {
            return headAnim;
        }

        public void LookAtLeft(bool left)
        {
            if (left)
            {
                asset.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                asset.transform.localScale = Vector3.one;
            }
        }

        public virtual void Run()
        {

        }

        public virtual void Walk()
        {

        }




    }
}
