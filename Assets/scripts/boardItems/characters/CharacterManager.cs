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
        [SerializeField] SpriteRenderer[] eyebrows;

        [SerializeField] PalettesManager.colorNames armsColor;
        [SerializeField] PalettesManager.colorNames legsColor;
        [SerializeField] PalettesManager.colorNames eyebrowsColor;

        public PalettesManager.colorNames GetArmsColor() { return armsColor; }
        public PalettesManager.colorNames GetLegsColor() { return legsColor; }
        public PalettesManager.colorNames GetEyebrowsColor() { return eyebrowsColor; }

        private void Awake()
        {
            Events.OnCharacterAnim += OnCharacterAnim;
            Events.OnCharacterPartAnim += OnCharacterPartAnim;
            Events.OnCharacterExpression += OnCharacterExpression;
            Events.ColorizeArms += ColorizeArms;
            Events.ColorizeLegs += ColorizeLegs;
            Events.ColorizeEyebrows += ColorizeEyebrows;
            Events.Zoom += Zoom;
        }
        private void OnDestroy()
        {
            Events.OnCharacterAnim -= OnCharacterAnim;
            Events.OnCharacterPartAnim -= OnCharacterPartAnim;
            Events.OnCharacterExpression -= OnCharacterExpression;
            Events.ColorizeArms -= ColorizeArms;
            Events.ColorizeLegs -= ColorizeLegs;
            Events.ColorizeEyebrows -= ColorizeEyebrows;
            Events.Zoom -= Zoom;
        }

        private void Zoom(CharacterPartsHelper.parts part, bool saving = false)
        {
            foreach (BodyPart p in bodyParts)
            {
                bool showBorders = p.part == part;
                if (saving)
                    showBorders = false;

                p.SetSelection(showBorders);
                p.GetComponent<Collider2D>().enabled = (showBorders);
            }
        }

        private void ColorizeArms(PalettesManager.colorNames colorName)
        {
            this.armsColor = colorName;
            foreach (SpriteRenderer sr in arms)
            {
                List<Color> allColors = Data.Instance.palettesManager.GetColorsByName(colorName);
                sr.color = allColors[0];
            }
        }
        private void ColorizeLegs(PalettesManager.colorNames colorName)
        {
            this.legsColor = colorName;
            foreach (SpriteRenderer sr in legs)
            {
                List<Color> allColors = Data.Instance.palettesManager.GetColorsByName(colorName);
                sr.color = allColors[0];
            }
        }
        private void ColorizeEyebrows(PalettesManager.colorNames colorName)
        {
            this.eyebrowsColor = colorName;
            foreach (SpriteRenderer sr in eyebrows)
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
            print("OnCharacter Anim " + anim);
            if (characterID != this.characterID) return;
            SetAnim(anim);
        }
        void OnCharacterPartAnim(int characterID, CharacterPartsHelper.parts part)
        {
            if (characterID != this.characterID) return;
            print("OnCharacter Part Anim " + part);
            anims.Play("edit_" + part.ToString().ToLower());
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
        public BodyPart GetBodyPart(CharacterPartsHelper.parts part)
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
