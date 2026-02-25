using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoardItems.Characters;

namespace Yaguar.StoryMaker.Editor
{
    public class AvatarFabulab : Avatar
    {

        public GameObject character_to_instantiate;
        public CharacterManager characterManager;

        private void Start()
        {
            StoryMakerEvents.SetAvatarData += SetData;
        }
        private void OnDestroy()
        {
            StoryMakerEvents.SetAvatarData -= SetData;
        }

        public override void OnInit() {


            string customizationSerialized = GetData().customizationSerialized;
                        
            if (asset != null)
                Destroy(asset.gameObject);

            asset = Instantiate(character_to_instantiate, transform);
            //asset.transform.SetParent(transform);
            asset.transform.localEulerAngles = Vector3.zero;
            asset.transform.localPosition = Vector3.zero;
            asset.SetActive(true);
            characterManager = asset.GetComponent<CharacterManager>();
        }
    }
}
