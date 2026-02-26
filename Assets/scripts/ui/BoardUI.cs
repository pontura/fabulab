using BoardItems;
using BoardItems.BoardData;
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
        public bool snap;
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
            OBJECT,
            NONE
        }
        public void SetEditingType(editingTypes t)  {
            this.editingType = t;
            switch (t)
            {   
                case editingTypes.SCENE:
                    sceneobjectsManager.gameObject.SetActive(true);
                    activeBoardItem = sceneobjectsManager;
                    break;
                case editingTypes.CHARACTER:
                    sceneobjectsManager.gameObject.SetActive(false);
                    characterManager.gameObject.SetActive(true);
                    activeBoardItem = characterManager;
                    break;
                case editingTypes.OBJECT:
                    characterManager.gameObject.SetActive(false);
                    sceneobjectsManager.gameObject.SetActive(true);
                    activeBoardItem = sceneobjectsManager;
                    break;
                default:
                    Debug.Log("# default");
                    characterManager.gameObject.SetActive(false);
                    sceneobjectsManager.gameObject.SetActive(false);
                    activeBoardItem.gameObject.SetActive(false);
                    break;
            }            
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
            //Events.EmptySceneItems += EmptySceneItems;
            Events.EmptyCharacterItemsButExlude += EmptyCharacterItemsButExlude;
        }

        private void OnDestroy()
        {
            Events.GalleryDone -= GalleryDone;
            //Events.EmptySceneItems -= EmptySceneItems;
            Events.EmptyCharacterItemsButExlude -= EmptyCharacterItemsButExlude;
        }
        void EmptySceneItems()
        {
            items.DeleteAll();
        }
        void EmptyCharacterItemsButExlude(CharacterPartsHelper.parts part) 
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
            CharacterData wd = Data.Instance.charactersData.SetCurrentID(id);
           // Data.Instance.galeriasData.SetGallery(wd.galleryID);
            OpenWork(wd);
            items.NextStepAnims(0, captureGifFramerate);
            Events.CaptureGif(id, callback);
        }
      
        public void LoadWork(string id)
        {
            items.DeleteAll();
            CharacterData cd;
            switch (editingType)
            {
                case editingTypes.CHARACTER:
                    cd = Data.Instance.charactersData.SetCurrentID(id);
                    break;
                default:
                    cd = Data.Instance.sObjectsData.SetCurrentID(id);
                    break;
            }
            if (cd != null)
                OpenWork(cd);
            else
                LoadOthersWork(id);
        }

        public void LoadOthersWork(string id) {
            items.DeleteAll();
            CharacterData cd;
            switch (editingType) {
                case editingTypes.CHARACTER:
                    Data.Instance.charactersData.LoadOthersCharacter(id, OpenWork);
                    break;
                default:
                    break;
            }
        }

        public void LoadPreset(CharacterPartData wd)
        {
            items.DeleteInPart(wd.items[0].part);
            OpenWork(wd);
        }

        void OpenWork(CharacterPartData wd) {
            items.OpenWork(wd);
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