using BoardItems.Characters;
using System.Collections.Generic;
using UnityEngine;

namespace BoardItems
{
    public class Items : MonoBehaviour
    {
        public AnimationClip GetItemAnim;
        public AnimationClip ReleaseItemAnim;
        public ItemInScene itemInScene_to_instantiate;
        float _z;
        float _z_value = 0.001f;
        ItemInScene itemSelected;
        public List<ItemInScene> all;
        public Inventary inventary;
        public SpriteRenderer bg;
        public Transform container;
        [SerializeField] CharacterManager characterManager;
        public CharacterManager CharacterManager { get { return characterManager; } }
        private void Awake()
        {
            characterManager.Init();
        }
        void Start()
        {
            Events.EditMode += EditMode;
            Events.OnStopDrag += OnStopDrag;
            Events.InitGallery += InitGallery;
            Events.Colorize += Colorize;
            Events.AnimateItem += AnimateItem;
            Events.ResetItems += ResetItems;

            characterManager.SetAnim(CharacterAnims.anims.edit);
            Events.EditMode(true);
        }
        void OnDestroy()
        {
            Events.EditMode -= EditMode;
            Events.OnStopDrag -= OnStopDrag;
            Events.InitGallery -= InitGallery;
            Events.Colorize -= Colorize;
            Events.AnimateItem -= AnimateItem;
            Events.ResetItems -= ResetItems;
        }
        void ResetItems()
        {
            StopAllCoroutines();
            int i = all.Count;
            while (i > 0)
            {
                ItemInScene itemInScene = all[i - 1];
                if (itemInScene.data.part > 0)
                {
                    bool wasMirrorer = Delete(itemInScene);
                    if (wasMirrorer)
                    {
                        all.Remove(all[i - 1]);
                        i--;
                    }
                    all.Remove(all[i - 1]);
                }
                i--;
            }
            inventary.Reset();
            //Utils.RemoveAllChildsIn(container);
        }
        public void ResetAllAnims()
        {
            foreach (ItemInScene itemInScene in all)
            {
                Animation anim = itemInScene.GetComponent<Animation>();
                if (anim != null && anim.GetClipCount() > 0)
                    if (anim.GetClip("myAnim") != null)
                        anim["myAnim"].normalizedTime = 0;
            }
        }
        void AnimateItem(AnimationsManager.AnimData anim)
        {
            DoAnimate(anim, itemSelected);
            if(IsInMirrorPart(itemSelected))
            {
                ItemInScene mirror = itemSelected.GetMirror();
                DoAnimate(anim, mirror);
            }
        }
        void DoAnimate(AnimationsManager.AnimData anim, ItemInScene item)
        { 
            ResetAllAnims();
            item.data.anim = anim.animName;
            Animation animComponent = item.GetComponent<Animation>();
            if (animComponent == null)
                animComponent = item.gameObject.AddComponent<Animation>();

            bool hasAnimSfx = false;
            if (anim.animName != AnimationsManager.anim.NONE)
            {
                int sfxCount = 0;
                foreach (ItemInScene iis in all)
                {
                    if (iis != item)
                    {
                        AnimSfx asfx = iis.GetComponent<AnimSfx>();
                        if (asfx != null)
                        {
                            if (asfx.animName == anim.animName)
                            {
                                hasAnimSfx = true;
                                break;
                            }
                            if (AudioManager.Instance.modSfx.Contains(asfx.animName))
                            {
                                sfxCount++;
                                if (sfxCount >= AudioManager.Instance.modSfx.MaxModeSfx)
                                {
                                    hasAnimSfx = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            AnimSfx aSfx = item.GetComponent<AnimSfx>();
            if (aSfx == null)
            {
                if (anim.animName != AnimationsManager.anim.NONE)
                {
                    aSfx = item.gameObject.AddComponent<AnimSfx>();
                    aSfx.animName = hasAnimSfx ? AnimationsManager.anim.NONE : anim.animName;
                }
            }
            else
            {
                aSfx.Init();
                aSfx.animName = hasAnimSfx ? AnimationsManager.anim.NONE : anim.animName;
            }


            animComponent.AddClip(anim.clip, "myAnim");
            animComponent.Play("myAnim");
        }

        public void NextStepAnims(int frameN, int frameRate)
        {
            float timeStep = 1f / frameRate;
            foreach (ItemInScene iis in all)
            {
                Animation anim = iis.GetComponent<Animation>();
                if (anim != null)
                {
                    if (anim["myAnim"] != null)
                    {
                        anim["myAnim"].time = frameN * timeStep;
                        anim["myAnim"].speed = 0;
                    }
                }
            }
        }

        void AnimateItemDragDrop(bool drag)
        {
            Animation animComponent = itemSelected.GetComponent<Animation>();
            if (animComponent == null)
                animComponent = itemSelected.gameObject.AddComponent<Animation>();

            if (animComponent.isPlaying) return;
            if (drag)
                animComponent.AddClip(GetItemAnim, "myAnim");
            else
                animComponent.AddClip(ReleaseItemAnim, "myAnim");
            animComponent.Play("myAnim");
        }
        void Colorize(PalettesManager.colorNames name)
        {
            itemSelected.SetColor(name);
            ItemInScene mirror = itemSelected.GetMirror();
            if (mirror != null) mirror.SetColor(name);
        }
        public ItemInScene GetItemSelected()
        {
            return itemSelected;
        }

        public void SetItemSelected(ItemInScene iInScene)
        {
            itemSelected = iInScene;
        }
        bool initialized = false;
        void InitGallery(GaleriasData.GalleryData gallery, bool editMode, System.Action OnAllLoaded)
        {
            print("InitGallery " + initialized);
            if (initialized)
            {
                if (OnAllLoaded != null)
                    OnAllLoaded();
            }
            else
            {
                initialized = true;
                print("InitGallery " + gallery.id);
                //all.Clear();
                StopAllCoroutines();
                bg.color = Data.Instance.palettesManager.GetColor(gallery.colorUI);
                inventary.Init(this, gallery.id, gallery.gallery, editMode, OnAllLoaded);
            }
        }
       
        ItemData InstantiateNewItem(int galleryID, ItemData itemData)
        {
            ItemData originalGO = Data.Instance.galeriasData.GetItem(galleryID, itemData.id);
            ItemData newGO = Instantiate(
                originalGO,
                originalGO.transform.position,
                Quaternion.identity
            );
            return newGO;
        }
        public void DeleteAll(CharacterData.parts exludePart)
        {
            StopAllCoroutines();
            
            int i = all.Count;
            while (i > 0)
            {
                ItemInScene itemInScene = all[i - 1];
                CharacterData.parts thisPart = itemInScene.data.part;

                if(thisPart == CharacterData.parts.HAND_LEFT)
                    thisPart = CharacterData.parts.HAND;
                if (thisPart == CharacterData.parts.FOOT_LEFT)
                    thisPart = CharacterData.parts.FOOT;

                if (thisPart != 0 && thisPart != exludePart)
                {
                    bool mirrorDeleted = Delete(itemInScene);
                    if (mirrorDeleted) i--;
                }
                i--;
            }
        }
        public void DeleteAll()
        {
            StopAllCoroutines();
            int i = all.Count;
            while (i > 0)
            {
                ItemInScene itemInScene = all[i - 1];
                bool mirrorDeleted = Delete(itemInScene);
                if (mirrorDeleted) i--;
                i--;
            }
        }
        public void DeleteInPart(int partID)
        {
            StopAllCoroutines();
            int i = all.Count;
            while (i > 0)
            {
                ItemInScene itemInScene = all[i - 1];
                if (itemInScene.data.part == (CharacterData.parts)partID)
                {
                    bool mirrorDeleted = Delete(itemInScene);
                    if (mirrorDeleted) i--;
                }
                i--;
            }
        }
        public void Delete()
        {
            Delete(itemSelected);
        }
        public void SetItemInScene(ItemInScene item, CharacterData.parts part)
        {
            characterManager.AttachItem(item);
            itemSelected = item;
        }
        void OnStopDrag(ItemInScene item, Vector3 pos)
        {
            item.StopBeingDrag();
            float posX = Camera.main.WorldToScreenPoint(pos).x;
            bool isOverBodyPart = item.IsOverBodyPart();
            if (!isOverBodyPart)
            {
                Delete(item);
                if (all.Count == 0)
                    Events.ActivateUIButtons(false);
            }
            else
            {
                SetItemInScene(item, item.data.part);
                item.data.position = item.transform.localPosition;
                item.data.rotation = item.transform.localEulerAngles;
                item.data.scale = item.transform.localScale;

                item.data.SetTransformByData();

                AnimateItemDragDrop(false);
                FinishEditingItem(item);
                Events.ActivateUIButtons(true);
                Events.SetChangesMade(true);

                BodyPart bp = characterManager.GetBodyPart(item.data.part);
                bp.SetArrengedItems();
            }
        }

        public void FinishEditingItem(ItemInScene item)
        {
            if (IsInMirrorPart(item))
                CheckMirror(item);
            else
            {
                ItemInScene mirror = item.GetMirror();
                if (mirror != null)
                {
                    item.SetMirror(null);
                    mirror.SetMirror(null);
                    Delete(mirror);
                }
            }
        }
        bool IsInMirrorPart(ItemInScene item)
        {
            //print(" IsInMirrorPart " + item.data.part);
            return item.data.part == CharacterData.parts.FOOT
                || item.data.part == CharacterData.parts.FOOT_LEFT
                || item.data.part == CharacterData.parts.HAND
                || item.data.part == CharacterData.parts.HAND_LEFT;
        }
        void CheckMirror(ItemInScene item)
        {
            ItemInScene mirror = item.GetMirror();
            if (mirror != null)
                EditMirror(item, mirror);
            else
                AddMirror(item);
        }
        void EditMirror(ItemInScene item, ItemInScene mirror)
        {
            switch(item.data.part)
            {
                case CharacterData.parts.FOOT:  mirror.data.part = CharacterData.parts.FOOT_LEFT; break;
                case CharacterData.parts.FOOT_LEFT: mirror.data.part = CharacterData.parts.FOOT; break;
                case CharacterData.parts.HAND: mirror.data.part = CharacterData.parts.HAND_LEFT; break;
                case CharacterData.parts.HAND_LEFT: mirror.data.part = CharacterData.parts.HAND; break;
            }
            SetItemInScene(mirror, mirror.data.part);

            mirror.transform.localPosition = item.transform.localPosition;
            mirror.transform.localEulerAngles = item.transform.localEulerAngles;
            mirror.transform.localScale = item.transform.localScale;

            mirror.data.position = item.transform.localPosition;
            mirror.data.rotation = item.transform.localEulerAngles;
            mirror.data.scale = item.transform.localScale;

        }
        void AddMirror(ItemInScene item)
        { 
            Vector3 pos = item.data.position;
            ItemInScene newItemInScene = Clonate(pos);
            item.SetMirror(newItemInScene);
            newItemInScene.SetMirror(item);
            EditMirror(item, newItemInScene);
        }
        public void StartDrag(ItemInScene item)
        {
            itemSelected = item;
            item.data.position = item.transform.position;


            if (!item.IsBeingUse())
            {
                _z -= _z_value;
                item.data.position.z = _z;
            }

            item.transform.position = item.data.position;

            item.StartBeingDrag();
            AnimateItemDragDrop(true);
        }
        public ItemInScene AddNewItemFromMenu(ItemInScene newItem)
        {
            this.itemSelected = newItem;
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = -0.1f;
            ItemInScene itemInScene =  Clonate(pos);
            return itemInScene;
        }
        public ItemInScene Clonate(Vector3 pos)
        {
            itemSelected.data.position = pos;
            ItemData newItem = InstantiateNewItem(itemSelected.data.galleryID, itemSelected.data);
            ItemInScene itemInScene = newItem.gameObject.AddComponent<ItemInScene>();
            newItem.galleryID = itemSelected.data.galleryID;
            newItem.colorName = itemSelected.data.colorName;
            newItem.id = itemSelected.data.id;
            newItem.position = pos;
            newItem.rotation = itemSelected.data.rotation;

            newItem.scale = itemSelected.data.scale;
            newItem.anim = itemSelected.data.anim;
            itemInScene.data = newItem;

            itemInScene.transform.position = pos;

            newItem.transform.localScale = itemSelected.data.scale;
            newItem.transform.SetParent(container);
            newItem.transform.rotation = itemSelected.transform.rotation;
            newItem.transform.position = itemSelected.data.position + new Vector3(0.2f, 0, 0);
            newItem.anim = itemSelected.data.anim;
            if (newItem.gameObject.GetComponent<Rigidbody2D>() == null)
            {
                Rigidbody2D rb = newItem.gameObject.AddComponent<Rigidbody2D>();
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            itemInScene.SetPos(newItem.transform.position);
            newItem.Init();
            all.Add(itemInScene);
            itemSelected = itemInScene;
            if (newItem.anim != AnimationsManager.anim.NONE)
            {
                AnimationsManager.AnimData animData = Data.Instance.animationsManager.GetAnimByName(itemSelected.data.anim);
                Events.AnimateItem(animData);
            }
            return itemInScene;
            //print("______4");
        }
        float back_z;
        public void MoveBack()
        {
            CharacterData.parts p = itemSelected.data.part;
            BodyPart bp = characterManager.GetBodyPart(p);
            bp.SendToBack(itemSelected);
            FinishEditingItem(itemSelected);
        }
        public void MoveUp()
        {
            CharacterData.parts p = itemSelected.data.part;
            BodyPart bp = characterManager.GetBodyPart(p);
            bp.SendToTop(itemSelected);
            FinishEditingItem(itemSelected);
        }
        public void ResetItemTransform()
        {
            itemSelected.data.rotation = Vector3.zero;
            itemSelected.transform.localEulerAngles = Vector3.zero;
            itemSelected.data.ResetScale();
            itemSelected.transform.localScale = itemSelected.data.scale;
            print("scale " + itemSelected.data.scale);
            FinishEditingItem(itemSelected);
        }
        public void RotateSnaped()
        {
            float z = itemSelected.data.rotation.z;
            z += Data.Instance.settings.snapAngle;
            z = Data.Instance.settings.snapAngle * (int)(z / Data.Instance.settings.snapAngle);
            itemSelected.data.rotation = new Vector3(0f, 0f, z);
            itemSelected.transform.localEulerAngles = itemSelected.data.rotation;
            FinishEditingItem(itemSelected);
        }
        public bool Delete(ItemInScene item)
        {
            print("delete " + item);
            ItemInScene mirror = item.GetMirror();
            if (mirror != null)
            {
                all.Remove(mirror);
                Destroy(mirror.gameObject);
            }
            all.Remove(item);
            Destroy(item.gameObject);
            return mirror != null;
        }
        //IEnumerator RemoveFromCanvas(float delay, ItemInScene item)
        //{
        //    yield return new WaitForEndOfFrame();
        //    if (item != null)
        //        item.StartFalling();
        //    all.Remove(item);
        //    yield return new WaitForSeconds(delay);
        //    if (item != null && item != itemSelected)
        //        Destroy(item.gameObject);
        //    if (all.Count == 0)
        //    {
        //        Events.ActivateUIButtons(false);
        //    }

        //}

        public void Rotate(float _x)
        {
            itemSelected.RotateSetValue(_x);
            FinishEditingItem(itemSelected);
        }
        public void Scale(float _x)
        {
            itemSelected.ScaleSetValue(_x);
            print("scale " + _x);
            FinishEditingItem(itemSelected);
        }
        void EditMode(bool isEditMode)
        {
            print("EditMode " + isEditMode);
            foreach (ItemInScene item in all)
                item.SetCollider(isEditMode);
        }

    }

}