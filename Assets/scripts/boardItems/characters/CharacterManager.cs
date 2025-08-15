using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;
using static UnityEditor.Progress;

namespace BoardItems.Characters
{
    public class CharacterManager : MonoBehaviour
    {
        public int characterID = 0;
        CharacterAnims anims;
        [SerializeField] BodyPart[] bodyParts;

        private void Awake()
        {
            Events.OnCharacterAnim += OnCharacterAnim;
        }
        private void OnDestroy()
        {
            Events.OnCharacterAnim -= OnCharacterAnim;
        }
        void OnCharacterAnim(int characterID, CharacterAnims.anims anim)
        {
            if (characterID != characterID) return;
            SetAnim(anim);
        }
        public void Init()
        {
            anims = GetComponent<CharacterAnims>();
        }
        public void SetAnim(CharacterAnims.anims anim)
        {
            anims.Play(anim);
        }
        public void AttachItem(ItemInScene item)
        {
            BodyPart bp = GetBodyPart(item.data.part);
            item.transform.SetParent(bp.transform);
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
