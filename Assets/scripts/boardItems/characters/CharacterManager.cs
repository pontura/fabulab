using System;
using UnityEngine;
using static AnimationsManager;

namespace BoardItems.Characters
{
    public class CharacterManager : MonoBehaviour
    {
        public int characterID = 0;
        CharacterAnims anims;
        CharacterExpressions expressions;
        [SerializeField] BodyPart[] bodyParts;

        private void Awake()
        {
            Events.OnCharacterAnim += OnCharacterAnim;
            Events.OnCharacterExpression += OnCharacterExpression;
        }
        private void OnDestroy()
        {
            Events.OnCharacterAnim -= OnCharacterAnim;
            Events.OnCharacterExpression -= OnCharacterExpression;
        }

        private void OnCharacterExpression(int characterID, CharacterExpressions.expressions exp)
        {
            if (characterID != characterID) return;
            expressions.Play(exp);
        }

        void OnCharacterAnim(int characterID, CharacterAnims.anims anim)
        {
            if (characterID != characterID) return;
            SetAnim(anim);
        }
        public void Init()
        {
            anims = GetComponent<CharacterAnims>();
            expressions = GetComponent<CharacterExpressions>();
        }
        public void SetAnim(CharacterAnims.anims anim)
        {
            anims.Play(anim);
        }
        public void AttachItem(ItemInScene item)
        {
            BodyPart bp = GetBodyPart(item.data.part);
            item.transform.SetParent(bp.transform);
            item.data.part = bp.part;
        }       
        BodyPart GetBodyPart(CharacterData.parts part)
        {
            foreach (BodyPart p in bodyParts)
            {
                if (p.part == part)
                    return p;
            }
            return null;
        }
    }
}
