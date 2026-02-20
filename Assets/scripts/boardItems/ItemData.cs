using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BoardItems.Characters;
using BoardItems.SceneObjects;

namespace BoardItems
{
    [Serializable]
    public class ItemData : MonoBehaviour
    {
        public CharacterPartsHelper.parts part;
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
       
        public void SetCharacterPart(CharacterPartsHelper.parts part)
        {
            this.part = part;
        }
        public void SetTransformByData()
        {
            transform.localPosition = position;
            transform.localEulerAngles = rotation;
            transform.localScale = scale;
        }
        public void ClonateFrom(ItemData itemDataOriginal)
        {
            part = itemDataOriginal.part;
            galleryID = itemDataOriginal.galleryID;
            rotation = itemDataOriginal.rotation;
            position = itemDataOriginal.position;
            scale = itemDataOriginal.scale;
            id = itemDataOriginal.id;
            paletteName = itemDataOriginal.paletteName;
            colorName = itemDataOriginal.colorName;
            anims = itemDataOriginal.anims;
            anim = itemDataOriginal.anim;
            originalScale = itemDataOriginal.originalScale;
        }
    }

}