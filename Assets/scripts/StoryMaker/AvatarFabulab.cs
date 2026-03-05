using BoardItems.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using static CharacterExpressions;

namespace Yaguar.StoryMaker.Editor
{
    public class AvatarFabulab : Avatar
    {

        public GameObject character_to_instantiate;
        public CharacterManager characterManager;

        private void Start()
        {
            StoryMakerEvents.SetAvatarData += SetData;
            Events.OnCharacterAnim += OnCharacterAnim;
            Events.OnCharacterExpression += OnCharacterExpression;
        }
        private void OnDestroy()
        {
            StoryMakerEvents.SetAvatarData -= SetData;
            Events.OnCharacterAnim -= OnCharacterAnim;
            Events.OnCharacterExpression -= OnCharacterExpression;
        }

        public override void OnInit() {

                        
            if (asset != null)
                Destroy(asset.gameObject);

            asset = Instantiate(character_to_instantiate, transform);
            //asset.transform.SetParent(transform);
            asset.transform.localEulerAngles = Vector3.zero;
            asset.transform.localPosition = Vector3.zero;
            asset.SetActive(true);
            characterManager = asset.GetComponent<CharacterManager>();
            characterManager.Init();

            Invoke("Delayed", 0.1f);            
        }
        void Delayed()
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            {
                bounds.Encapsulate(sr.bounds);
            }

            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
            Vector3 localSize = transform.InverseTransformVector(bounds.size);

            collider.offset = localCenter;
            collider.size = localSize;
        }

        public override void Run() {
            characterManager.SetAnim(CharacterAnims.anims.groovyWalk_right);
        }

        public override void Walk() {
            characterManager.SetAnim(CharacterAnims.anims.walk_right);
        }

        void OnCharacterExpression(string _characterID, CharacterExpressions.expressions exp) {
            if (data.id != _characterID) return;
            (data as SOAvatarFabulabData).emoji = exp;
        }

        void OnCharacterAnim(string _characterID, CharacterAnims.anims anim) {
            if(data.id != _characterID) return;
            (data as SOAvatarFabulabData).anim = anim;
        }
    }
}
