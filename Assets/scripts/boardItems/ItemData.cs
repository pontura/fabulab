using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BoardItems.Characters;

namespace BoardItems
{
    [Serializable]
    public class ItemData : MonoBehaviour
    {
        public CharacterData.parts part;
        public int galleryID;
        public Vector3 rotation;
        public Vector3 position;
        public Vector3 scale;
        public int id;
        public PalettesManager.paletteNames paletteName;
        public PalettesManager.colorNames colorName;
        public AnimationsManager.anim[] anims;
        public AnimationsManager.anim anim;
        public Vector3 originalScale;

        public void Init()
        {
            this.rotation = transform.localEulerAngles;
            this.scale = transform.localScale;
            originalScale = scale;
        }
        public void ResetScale()
        {
            this.scale = originalScale;
        }
        [SerializeField] int partsCount;
        public int PartsCount { get { return partsCount; } }

        public void OutOfBody()
        {
            partsCount--;
        }
        public void SetCharacterPart(CharacterData.parts part)
        {
            if (part != CharacterData.parts.none)
                partsCount++;
            this.part = part;
        }
    }

}