using BoardItems;
using BoardItems.Characters;
using System.Collections;
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
        public CharacterManager characterManager;

        int captureGifFramerate = 40;

        private void Awake()
        {
            characterManager.Init();
        }
        private void Start()
        {
            characterManager.SetAnim(CharacterAnims.anims.edit);
            Events.GalleryDone += GalleryDone;
            Events.EmptyCharacterItems += EmptyCharacterItems;
            Events.EmptyCharacterItemsButExlude += EmptyCharacterItemsButExlude;
        }

        private void OnDestroy()
        {
            Events.GalleryDone -= GalleryDone;
            Events.EmptyCharacterItems -= EmptyCharacterItems;
            Events.EmptyCharacterItemsButExlude -= EmptyCharacterItemsButExlude;
        }
        void EmptyCharacterItems()
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
            characterManager.AttachItem(item);
        }
        public void OnStopDrag(ItemInScene item)
        {
            BodyPart bp = characterManager.GetBodyPart(item.data.part);
            bp.SetArrengedItems();
        }
        public void MoveBack(ItemInScene itemSelected)
        {
            CharacterData.parts p = itemSelected.data.part;
            BodyPart bp = characterManager.GetBodyPart(p);
            bp.SendToBack(itemSelected);
        }
        public void MoveUp(ItemInScene itemSelected)
        {
            CharacterData.parts p = itemSelected.data.part;
            BodyPart bp = characterManager.GetBodyPart(p);
            bp.SendToTop(itemSelected);
        }

    }

}