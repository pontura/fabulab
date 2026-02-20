using BoardItems;
using BoardItems.Characters;
using BoardItems.SceneObjects;
using System.Collections;
using UI.MainApp;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace UI
{
    public class BoardUI : MonoBehaviour
    {
        public GameObject zoomBar;
        public Items items;
        public ToolsMenu toolsMenu;
        public GameObject colorPickerContainer;
        public Transform colorPickerContent;
        public GameObject colorPicketBtnPrefab;
        public Camera cam;
        public Screenshot screenshot;

        BoardItemManager activeBoardItem;

        public CharacterManager characterManager;
        public SceneObjectManager sceneobjectsManager;

        public editingTypes editingType;
        public enum editingTypes
        {
            SCENE,
            CHARACTER,
            OBJECT
        }
        public void SetEditingType(editingTypes t)  {
            this.editingType = t;
            switch (t)
            {   
                case editingTypes.SCENE:
                    sceneobjectsManager.gameObject.SetActive(false);
                    activeBoardItem = characterManager;
                    break;
                case editingTypes.CHARACTER:
                    sceneobjectsManager.gameObject.SetActive(false);
                    activeBoardItem = characterManager;
                    break;
                case editingTypes.OBJECT:
                    characterManager.gameObject.SetActive(false);
                    activeBoardItem = sceneobjectsManager;
                    break;
                default:
                    break;
            }
            activeBoardItem.gameObject.SetActive(true);
        }

        int captureGifFramerate = 40;

        private void Awake()
        {
            characterManager.Init();
        }
        private void Start()
        {

            CharacterAnims.anims anim = CharacterAnims.anims.edit;
            Events.OnCharacterAnim(0, anim);

            Events.GalleryDone += GalleryDone;
            Events.EmptySceneItems += EmptySceneItems;
            Events.EmptyCharacterItemsButExlude += EmptyCharacterItemsButExlude;
        }

        private void OnDestroy()
        {
            Events.GalleryDone -= GalleryDone;
            Events.EmptySceneItems -= EmptySceneItems;
            Events.EmptyCharacterItemsButExlude -= EmptyCharacterItemsButExlude;
        }
        void EmptySceneItems()
        {
            items.DeleteAll();
        }
        void EmptyCharacterItemsButExlude(CharacterData.parts part) 
        {
            items.DeleteAll(part);
        }
        void GalleryDone()
        {
            AudioManager.Instance.musicManager.Play("board");
        }
       
        void OnResetConfirmed(bool confirmed)
        {
            if(confirmed)
                ResetBoardConfirmed();
        }
        public void ResetBoardConfirmed()
        {
            toolsMenu.SetOff();
            items.DeleteAll();
        }

        public void SetBgColorPicker(bool enable)
        {
            if (enable)
            {
                AudioManager.Instance.uiSfxManager.Play("pop", 0.5f);
                foreach (Transform child in colorPickerContent)
                    Destroy(child.gameObject);

                int galleryID =1;

                if (items.GetItemSelected() != null)
                    galleryID = items.GetItemSelected().data.galleryID;

                PalettesManager.colorNames[] names = Data.Instance.galeriasData.GetGallery(galleryID).colors;
                foreach (PalettesManager.colorNames n in names)
                {
                    GameObject go = Instantiate(colorPicketBtnPrefab, colorPickerContent);
                    go.GetComponent<Image>().color = Data.Instance.palettesManager.GetColorsByName(n)[0];
                    go.GetComponent<Button>().onClick.AddListener(() => ClickBgColorBtn(go.GetComponent<Image>().color));
                }

            }
            else
            {
                AudioManager.Instance.uiSfxManager.PlayTransp("pop", -2, 0.5f);
            }
            //BgColorBtn.SetActive(!enable);
            colorPickerContainer.SetActive(enable);
        }


        //public void ActivateTilde(bool enable)
        //{
        //    DoneBtn.SetActive(enable);
        //    zoomBar.SetActive(enable);
        //}

        public void ClickBgColorBtn(Color c)
        {
            AudioManager.Instance.uiSfxManager.PlayNextScale("click");
            SetBgColor(c);
        }

        public void SetBgColor(Color color)
        {
            cam.backgroundColor = color;
        }

        public IEnumerator CreateGif(string id, System.Action callback)
        {
            yield return new WaitForSeconds(1);
            AlbumData.CharacterData wd = Data.Instance.albumData.SetCurrentID(id);
           // Data.Instance.galeriasData.SetGallery(wd.galleryID);
            OpenWork(wd);
            items.NextStepAnims(0, captureGifFramerate);
            Events.CaptureGif(id, callback);
        }
      
        public void LoadWork(string id)
        {
            items.DeleteAll();
            AlbumData.CharacterData wd = Data.Instance.albumData.SetCurrentID(id);
           // UIManager.Instance.gallerySelectorUI.SelectGallery(wd.galleryID, true);
           //TO-DO:
            //List<int> galleries = new List<int>();
            //foreach(AlbumData.CharacterData.SavedIData item in wd.items)
            //{
            //    if (!galleries.Contains(item.galleryID))
            //        galleries.Add(item.galleryID);
            //}
            //UIManager.Instance.gallerySelectorUI.SelectGallery(galleries);
            OpenWork(wd);
        }
        public void LoadPreset(AlbumData.CharacterData wd)
        {
            items.DeleteInPart(wd.items[0].part);
            OpenWork(wd);
        }
        ItemData CreateItem(AlbumData.CharacterData.SavedIData itemData)
        {
            ItemData originalGO = Data.Instance.galeriasData.GetItem(itemData.galleryID, itemData.id);
            print("____________" + originalGO.name);
            ItemData newItem = Instantiate(
                originalGO,
                originalGO.transform.position,
                Quaternion.identity
            );
            //return newGO;
            //ItemData newItem = Instantiate(Resources.Load<ItemData>("galerias/" + itemData.galleryID + "/item_" + itemData.id));
            // Debug.Log("ID" + itemData.id + ":" + itemData.position);
            newItem.galleryID = itemData.galleryID;
            newItem.part = (CharacterData.parts)itemData.part;
            newItem.position = itemData.position;
            newItem.rotation = itemData.rotation;
            newItem.scale = itemData.scale;
            newItem.colorName = itemData.color;
            newItem.anim = itemData.anim;

            ItemInScene itemInScene = newItem.gameObject.AddComponent<ItemInScene>();
            itemInScene.SetCollider(false);
            itemInScene.data = newItem;
            items.all.Add(itemInScene);
            items.SetItemInScene(itemInScene, newItem.part);
            itemInScene.data.SetTransformByData();

            items.FinishEditingItem(itemInScene);
            return newItem;
        }



        void OpenWork(AlbumData.CharacterData wd)
        {
            StartCoroutine(OpenWork_C(wd));
        }
        IEnumerator OpenWork_C(AlbumData.CharacterData wd)
        {
            print("open work");
            foreach (AlbumData.CharacterData.SavedIData itemData in wd.items)
            {
                yield return new WaitForSeconds(0.05f);
                ItemData newItem = CreateItem(itemData);

                print("open work newItem part: " + newItem.part);

                if (newItem.anim != AnimationsManager.anim.NONE)
                {
                    AnimationsManager.AnimData animData = Data.Instance.animationsManager.GetAnimByName(newItem.anim);
                    Events.AnimateItem(animData);
                }
                newItem.GetComponent<ItemInScene>().Appear();
            }
            Events.ColorizeArms( wd.armsColor );
            Events.ColorizeLegs( wd.legsColor );
            Events.ColorizeEyebrows( wd.eyebrowsColor );
        }
        public void AttachItem(ItemInScene item)
        {
            activeBoardItem.AttachItem(item);
        }
        public void OnStopDrag(ItemInScene item)
        {
            activeBoardItem.OnStopDrag(item);
        }
        public void MoveBack(ItemInScene item)
        {
            activeBoardItem.MoveBack(item);
        }
        public void MoveUp(ItemInScene item)
        {
            activeBoardItem.MoveUp(item);
        }

    }

}