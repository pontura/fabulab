using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoardItems.Characters
{
    public class CharacterManager : MonoBehaviour
    {
        public int characterID = 0;
        CharacterAnims anims;
        CharacterExpressions expressions;
        [SerializeField] BodyPart[] bodyParts;
        [SerializeField] SpriteRenderer[] arms;
        [SerializeField] SpriteRenderer[] legs;

        private void Awake()
        {
            Events.OnCharacterAnim += OnCharacterAnim;
            Events.OnCharacterExpression += OnCharacterExpression;
            Events.ColorizeArms += ColorizeArms;
            Events.ColorizeLegs += ColorizeLegs;
            Events.Zoom += Zoom;
        }
        private void OnDestroy()
        {
            Events.OnCharacterAnim -= OnCharacterAnim;
            Events.OnCharacterExpression -= OnCharacterExpression;
            Events.ColorizeArms -= ColorizeArms;
            Events.ColorizeLegs -= ColorizeLegs;
            Events.Zoom -= Zoom;
        }

        private void Zoom(CharacterData.parts part)
        {
            foreach (BodyPart p in bodyParts)
            {
                p.SetSelection(p.part == part);
                p.GetComponent<Collider2D>().enabled = (p.part == part);
            }
        }

        private void ColorizeArms(PalettesManager.colorNames colorName)
        {
            foreach (SpriteRenderer sr in arms)
            {
                List<Color> allColors = Data.Instance.palettesManager.GetColorsByName(colorName);
                sr.color = allColors[0];
            }
        }
        private void ColorizeLegs(PalettesManager.colorNames colorName)
        {
            foreach (SpriteRenderer sr in legs)
            {
                List<Color> allColors = Data.Instance.palettesManager.GetColorsByName(colorName);
                sr.color = allColors[0];
            }
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
        public BodyPart GetBodyPart(CharacterData.parts part)
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
