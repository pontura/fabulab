using BoardItems.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

            BoxCollider2D collider = GetComponent<BoxCollider2D>();

            Bounds bounds = new Bounds(transform.position, Vector3.zero);

            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>()) {
                bounds.Encapsulate(sr.bounds);
            }

            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
            Vector3 localSize = transform.InverseTransformVector(bounds.size);

            collider.offset = localCenter;
            collider.size = localSize;

        }
    }
}
