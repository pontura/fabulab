using BoardItems.BoardData;
using BoardItems.Characters;
using BoardItems.SceneObjects;
using System.Collections;
using System.Collections.Generic;
using UI;
using UI.MainApp;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Yaguar.StoryMaker.Editor;
using static UnityEngine.GraphicsBuffer;

namespace BoardItems
{
    public class Items : MonoBehaviour
    {
        public AnimationClip GetItemAnim;
        public AnimationClip ReleaseItemAnim;
        public ItemInScene itemInScene_to_instantiate;
        float _z;

        [SerializeField] ItemInScene itemSelected;
        [SerializeField] ItemInScene newItemFromMenu;

        public Inventary inventary;
        public Transform container;

        void Start()
        {
            Events.EditMode += EditMode;
            Events.OnStopDrag += OnStopDrag;
            Events.InitGallery += InitGallery;            
            Events.Colorize += Colorize;
            Events.AnimateItem += AnimateItem;
            Events.ResetItems += ResetItems;
            Events.LoadBoardItemForStory += LoadBoardItemForStory;
            Events.LoadBgForStory += LoadBgForStory;
            Events.ShowScreen += ShowScreen;
        }
        public bool storyMode;
        private void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.StoryMaker:
                    storyMode = true;
                    break;
                default:
                    storyMode = false;
                    break;
            }
        }
        void OnDestroy()
        {
            Events.EditMode -= EditMode;
            Events.OnStopDrag -= OnStopDrag;
            Events.InitGallery -= InitGallery;
            Events.Colorize -= Colorize;
            Events.AnimateItem -= AnimateItem;
            Events.ResetItems -= ResetItems;
            Events.LoadBoardItemForStory -= LoadBoardItemForStory;
            Events.ShowScreen -= ShowScreen;
        }
        public List<ItemInScene> all
        {
            get
            {
                List<ItemInScene> a = new List<ItemInScene>();
                if (UIManager.Instance.boardUI.AllBodyParts == null) return a;
                foreach (BodyPart bp in UIManager.Instance.boardUI.AllBodyParts)
                {
                    foreach (ItemInScene i in bp.GetComponentsInChildren<ItemInScene>())
                    {
                        a.Add(i);
                    }
                }
                return a;
            }
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
                        i--;
                    }
                }
                i--;
            }
            inventary.Reset();
        }

        void AnimateItem(AnimationsManager.AnimData anim)
        {
            DoAnimate(anim, itemSelected);
        }
        void DoAnimate(AnimationsManager.AnimData anim, ItemInScene item)
        {
          //  ResetAllAnims();
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

            //AnimSfx aSfx = item.GetComponent<AnimSfx>();
            //if (aSfx == null)
            //{
            //    if (anim.animName != AnimationsManager.anim.NONE)
            //    {
            //        aSfx = item.gameObject.AddComponent<AnimSfx>();
            //        aSfx.animName = hasAnimSfx ? AnimationsManager.anim.NONE : anim.animName;
            //    }
            //}
            //else
            //{
            //    aSfx.Init();
            //    aSfx.animName = hasAnimSfx ? AnimationsManager.anim.NONE : anim.animName;
            //}


            //animComponent.AddClip(anim.clip, "myAnim");
            //animComponent.Play("myAnim");
        }

        public void NextStepAnims(int frameN, int frameRate)
        {
            //float timeStep = 1f / frameRate;
            //foreach (ItemInScene iis in all)
            //{
            //    Animation anim = iis.GetComponent<Animation>();
            //    if (anim != null)
            //    {
            //        if (anim["myAnim"] != null)
            //        {
            //            anim["myAnim"].time = frameN * timeStep;
            //            anim["myAnim"].speed = 0;
            //        }
            //    }
            //}
        }

        void AnimateItemDragDrop(bool drag)
        {
            Animation animComponent = itemSelected.GetComponent<Animation>();
            if (animComponent == null)
                animComponent = itemSelected.gameObject.AddComponent<Animation>();

            //if (animComponent.isPlaying) return;
            //if (drag)
            //    animComponent.AddClip(GetItemAnim, "myAnim");
            //else
            //    animComponent.AddClip(ReleaseItemAnim, "myAnim");
            //animComponent.Play("myAnim");
        }
        void Colorize(PalettesManager.colorNames name)
        {
            print("COLOR " + name);
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
            if(!IsAMirroredPart(iInScene))
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
        public void DeleteAll(CharacterPartsHelper.parts exludePart)
        {
            StopAllCoroutines();

            int i = all.Count;
            while (i > 0)
            {
                ItemInScene itemInScene = all[i - 1];
                CharacterPartsHelper.parts thisPart = itemInScene.data.part;

                if (thisPart == CharacterPartsHelper.parts.HAND_LEFT)
                    thisPart = CharacterPartsHelper.parts.HAND;
                if (thisPart == CharacterPartsHelper.parts.FOOT_LEFT)
                    thisPart = CharacterPartsHelper.parts.FOOT;

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
                if (itemInScene != null)
                    Delete(itemInScene);
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
                if (itemInScene != null)
                {
                    if (itemInScene.data.part == (CharacterPartsHelper.parts)partID)
                        Delete(itemInScene);
                }
                i--;
            }
        }
        public void Delete()
        {
            Delete(itemSelected);
        }
        public void SetItemInScene(BoardItemManager target, ItemInScene item, CharacterPartsHelper.parts part)
        {
            target.AttachItem(item);
            SetItemSelected(item);
        }
        public bool snap = true;
        void OnStopDrag(ItemInScene item, Vector3 pos)
        {
            newItemFromMenu = null;
            if (item == null) return;
            item.StopBeingDrag();
            UIManager.Instance.undoManager.OnNewStep();
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
                SetItemInScene(item.data.BoardItemManager, item, item.data.part);
                item.data.position = item.transform.localPosition;
                item.data.rotation = item.transform.localEulerAngles;
                item.data.scale = item.transform.localScale;

                item.data.SetTransformByData();

                AnimateItemDragDrop(false);
                FinishEditingItem(item);
                Events.ActivateUIButtons(true);
                Events.SetChangesMade(true);

                item.data.BoardItemManager.OnStopDrag(item);
            }
        }

        public void FinishEditingItem(ItemInScene item)
        {
            if (IsInMirrorPart(item))
                CheckMirror(item);
        }
        bool IsInMirrorPart(ItemInScene item)
        {
            return item.data.part == CharacterPartsHelper.parts.FOOT
                || item.data.part == CharacterPartsHelper.parts.HAND;
        }
        bool IsAMirroredPart(ItemInScene item)
        {
            return item.data.part == CharacterPartsHelper.parts.FOOT_LEFT
                || item.data.part == CharacterPartsHelper.parts.HAND_LEFT;
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
            switch (item.data.part)
            {
                case CharacterPartsHelper.parts.FOOT: mirror.data.part = CharacterPartsHelper.parts.FOOT_LEFT; break;
                case CharacterPartsHelper.parts.HAND: mirror.data.part = CharacterPartsHelper.parts.HAND_LEFT; break;
            }
            SetItemInScene(item.data.BoardItemManager, mirror, mirror.data.part);


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

            SetItemSelected(item);

            item.data.position = item.transform.localPosition;


            if (item == newItemFromMenu)
            {
                _z = -1;
                item.data.position.z = _z;
            }

            item.transform.localPosition = item.data.position;

            item.StartBeingDrag();
            AnimateItemDragDrop(true);
        }
        public ItemInScene AddNewItemFromMenu(ItemInScene newItem)
        {
            SetItemSelected(newItem);
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = -0.1f;
            ItemInScene itemInScene = Clonate(pos);
            newItemFromMenu = itemInScene;
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

            SetItemSelected(itemInScene);

            if (newItem.anim != AnimationsManager.anim.NONE)
            {
                AnimationsManager.AnimData animData = Data.Instance.animationsManager.GetAnimByName(itemSelected.data.anim);
                Events.AnimateItem(animData);
            }
            return itemInScene;
        }
        float back_z;
        public void MoveBack()
        {
            itemSelected.data.BoardItemManager.MoveBack(itemSelected);
            //board.MoveBack(itemSelected);
            FinishEditingItem(itemSelected);
        }
        public void MoveUp()
        {
            itemSelected.data.BoardItemManager.MoveUp(itemSelected);
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
                Destroy(mirror.gameObject);
            }
            Destroy(item.gameObject);
            return mirror != null;
        }
        public void ScaleSnaped(bool up)
        {
            float snapScale = Data.Instance.settings.snapScale;
            float scale = itemSelected.data.scale.x;

            if (up)
                scale += snapScale;
            else
                scale -= snapScale;

            if (scale < snapScale) scale = snapScale;

            scale = snapScale * (scale / snapScale);
            ScaleSnaped(scale);
        }
        public void ScaleSnaped(float scale)
        {
            itemSelected.data.scale = new Vector3(scale, scale, scale);
            itemSelected.transform.localScale = itemSelected.data.scale;
            FinishEditingItem(itemSelected);
        }
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
                    
        void LoadBgForStory(BoardItemManager itemManager, string id, Transform bgContainer, Transform container) {
            LoadBoardItemForStory(itemManager, id);
            LoadBgSos(itemManager, bgContainer, container);
        }

        void LoadBoardItemForStory(BoardItemManager itemManager, string id)
        {
            SOPartData pd = null;
            if (itemManager is CharacterManager) {
                pd = Data.Instance.charactersData.SetCurrentID(id);
                if (pd == null) {
                    Data.Instance.charactersData.LoadOthersCharacter(id, (partData) => OpenWork(itemManager,partData));
                    return;
                }
            } else if (itemManager is SceneObjectManager) {
                pd = Data.Instance.sObjectsData.SetCurrentID(id);
                if (pd == null) {
                    Data.Instance.sObjectsData.LoadOthersObject(id, (partData) => {
                        if (partData is SObjectData soData) {
                            if (soData.type == SObjectData.types.background)
                                Events.ColorizeBG(soData.bg);
                        }
                        OpenWork(itemManager, partData);
                        });
                    return;
                }
                if (pd is SObjectData soData) {
                    if (soData.type == SObjectData.types.background)
                        Events.ColorizeBG(soData.bg);
                }
            }

            OpenWork(itemManager, pd);
        }

        ItemData CreateItem(SavedIData itemData, BoardItemManager target)
        {
            ItemData originalGO = Data.Instance.galeriasData.GetItem(itemData.galleryID, itemData.id);
            ItemData newItem = Instantiate(
                originalGO
            );
            newItem.galleryID = itemData.galleryID;
            newItem.part = (CharacterPartsHelper.parts)itemData.part;
            newItem.position = itemData.position;
            newItem.rotation = itemData.rotation;
            newItem.scale = itemData.scale;
            newItem.colorName = itemData.color;
            newItem.anim = itemData.anim;

            ItemInScene itemInScene = newItem.gameObject.AddComponent<ItemInScene>();
            itemInScene.SetCollider(false);
            itemInScene.data = newItem;

            SetItemInScene(target, itemInScene, newItem.part);
            itemInScene.data.SetTransformByData();

            FinishEditingItem(itemInScene);
            return newItem;
        }
        public void AddSceneObjectTo(SOPartData wd, BoardItemManager newBoardItemManager, Transform container, SavedIData savedIOData = null)
        {
            BoardItemManager boardItemManager = Instantiate(newBoardItemManager, container);
            boardItemManager.name = "Object " + wd.id;
            boardItemManager.transform.SetParent(container);
            boardItemManager.transform.localPosition = Vector3.zero;
            ItemData newItem;
            List<GameObject> childs = new List<GameObject>();
            float _z = 0;
            foreach (SavedIData itemData in wd.items)
            {
                _z -= 0.001f;
                itemData.position.z = _z;
                newItem = CreateItem(itemData, newBoardItemManager);
                newItem.transform.SetParent(boardItemManager.transform);
                childs.Add(newItem.gameObject);
            }
            boardItemManager.transform.localScale = new Vector3(8,8,1);
            foreach(GameObject go in childs)
                go.transform.SetParent(transform);
            boardItemManager.transform.localScale = new Vector3(1, 1, 1);
            foreach (GameObject go in childs)
                go.transform.SetParent(boardItemManager.transform);

            //float snapScale = Data.Instance.settings.snapScale*8;
            //float scale = snapScale * (8 / snapScale);
            //UIManager.Instance.boardUI.items.ScaleSnaped(-snapScale);

            boardItemManager.SetInteractableObject(wd.id,  OnObjectMerged, savedIOData);


        }
        void OnObjectMerged(BoardItemManager boardItemManager)
        {
            BodyPart bodyPart = boardItemManager.GetComponentInParent<BodyPart>();
            if (bodyPart != null)
                bodyPart.SetArrengedItems();
        }
        List<SavedIData> soIDs;
        public void OpenWork(BoardItemManager boardItemManager, SOPartData wd, bool cascade = false)
        {
            soIDs = new List<SavedIData>();
            if (Data.Instance.charactersData.PresetID == "" && cascade)
                Events.ColorizeBG(wd.bg);
            if (wd is CharacterData characterData)
            {
                Events.ColorizeArms(cascade ? "" : wd.id, characterData.armsColor);
                Events.ColorizeLegs(cascade ? "" : wd.id, characterData.legsColor);
                Events.ColorizeEyebrows(cascade ? "" : wd.id, characterData.eyebrowsColor);
            }

            print("open work boardItemManager: " + boardItemManager.name + " id: " + wd.id + " cascade: " + cascade);

            foreach (SavedIData itemData in wd.items)
            {
                if (itemData.soID != "" && itemData.soID != null)
                {
                    soIDs.Add(itemData);                   
                }
                else
                {
                    ItemData newItem = CreateItem(itemData, boardItemManager);
                    print("open work newItem part: " + newItem.part);
                }
                //if (newItem.anim != AnimationsManager.anim.NONE)
                //{
                //    AnimationsManager.AnimData animData = Data.Instance.animationsManager.GetAnimByName(newItem.anim);
                //    Events.AnimateItem(animData);
                //}
                //newItem.GetComponent<ItemInScene>().Appear();
            }
            if (cascade)
                StartCoroutine(Cascade(boardItemManager, wd));            
        }

        void LoadBgSos(BoardItemManager boardItemManager, Transform bgContainer, Transform container) {            
            foreach (SavedIData soID in soIDs) {
                print("open SO in background soID: " + soID.soID);
                SOPartData soPartData = Data.Instance.sObjectsData.GetSO(soID.soID);
                if (soPartData != null) {
                    SOPartData o = Data.Instance.sObjectsData.GetSO(soID.soID);
                    GameObject go = new GameObject();
                    BoardItemManager boardItemManager_to_add = go.AddComponent<BoardItemManager>();

                    if (container == null)
                        container = boardItemManager.GetBodyPart((CharacterPartsHelper.parts)soID.part).transform;
                    Destroy(go.gameObject);

                    //AddSceneObjectTo(o, boardItemManager_to_add, container, soID);
                    //AddSceneObjectTo(SOPartData wd, BoardItemManager newBoardItemManager, Transform container, SavedIData savedIOData = null)
         
                    BoardItemManager childBoardItemManager = Instantiate(boardItemManager_to_add, container);
                    childBoardItemManager.name = "Object " + o.id;
                    childBoardItemManager.transform.SetParent(container);
                    childBoardItemManager.transform.localPosition = Vector3.zero;
                    ItemData newItem;
                    List<GameObject> childs = new List<GameObject>();
                    float _z = 0;
                    foreach (SavedIData itemData in o.items) {
                        _z -= 0.001f;
                        itemData.position.z = _z;
                        newItem = CreateItem(itemData, boardItemManager_to_add);
                        newItem.transform.SetParent(childBoardItemManager.transform);
                        childs.Add(newItem.gameObject);
                    }

                    float scale = bgContainer.localScale.x;
                    float offsetY = bgContainer.localPosition.y;
                    childBoardItemManager.transform.localScale = new Vector3(8 * scale, 8 * scale, 1);
                    childBoardItemManager.transform.localPosition = new Vector3(0, offsetY, 0);
                    
                    foreach (GameObject gO in childs)
                        gO.transform.SetParent(transform);
                    
                    childBoardItemManager.transform.localScale = new Vector3(1f, 1f, 1f);
                    childBoardItemManager.transform.localPosition = new Vector3(childBoardItemManager.transform.localPosition.x,0,0);
                    foreach (GameObject gO in childs)
                        gO.transform.SetParent(childBoardItemManager.transform);


                    //float snapScale = Data.Instance.settings.snapScale*8;
                    //float scale = snapScale * (8 / snapScale);
                    //UIManager.Instance.boardUI.items.ScaleSnaped(-snapScale);

                    childBoardItemManager.SetInteractableObject(o.id, OnObjectMerged, soID);
                } else print("open work itemData.soID not found: " + soID);
            }
            if(soIDs.Count>0)
                bgContainer.localScale *= 0.5f;
        }

        IEnumerator Cascade(BoardItemManager boardItemManager, SOPartData wd)
        {
            CharacterPartsHelper.parts partActive = UIManager.Instance.part;
            foreach (ItemInScene i in boardItemManager.GetComponentsInChildren<ItemInScene>())
            {
                if(i.data.part == partActive || partActive == CharacterPartsHelper.parts.none)
                    i.Appear();
            }
            yield return new WaitForSeconds(0.05f);
            foreach (ItemInScene i in boardItemManager.GetComponentsInChildren<ItemInScene>())
            {
                if (i.data.part == partActive || partActive == CharacterPartsHelper.parts.none)
                {
                    i.AppearAction();
                    yield return new WaitForSeconds(0.05f);
                }
                if(i.data.part == partActive)
                    i.SetCollider(true);
            }
            yield return new WaitForSeconds(0.05f);
            foreach (SavedIData soID in soIDs) {
                print("open SO in background soID: " + soID.soID);
                SOPartData soPartData = Data.Instance.sObjectsData.GetSO(soID.soID);
                if (soPartData != null) {
                    SOPartData o = Data.Instance.sObjectsData.GetSO(soID.soID);
                    GameObject go = new GameObject();
                    BoardItemManager boardItemManager_to_add = go.AddComponent<BoardItemManager>();                    
                    
                    Destroy(go.gameObject);
                    AddSceneObjectTo(o, boardItemManager_to_add, boardItemManager.GetBodyPart((CharacterPartsHelper.parts)soID.part).transform, soID);
                } else print("open work itemData.soID not found: " + soID);
            }
        }
        public void SetColliders(BoardItemManager boardItemManager, bool isOn)
        {
            foreach (ItemInScene i in boardItemManager.GetComponentsInChildren<ItemInScene>())
            {
                i.SetCollider(isOn);
            }
        }

    }

}
