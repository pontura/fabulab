using BoardItems.Characters;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BoardItems.UI
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
        public GameObject BackBtn;
        public GameObject DoneBtn;
        public GameObject ResetBtn;
        public GameObject CanvasMenu;
        public Image[] allColorizableSprites;
        public Screenshot screenshot;

        int captureGifFramerate = 40;

        private void Start()
        {
            Events.InitGallery += InitGallery;
            Events.ActivateUIButtons += ActivateUIButtons;
        }

        private void OnDestroy()
        {
            Events.InitGallery -= InitGallery;
            Events.ActivateUIButtons -= ActivateUIButtons;
        }

        public void BackToGallerySelector()
        {
            //AudioManager.Instance.uiSfxManager.PlayUISfx("back");
            //AudioManager.Instance.uiSfxManager.PlayPrevScaleUISfx("click");
            AudioManager.Instance.uiSfxManager.PlayTransp("click", -3);
            UIManager.Instance.gallerySelectorUI.ShowGallerySelector(true);
        }

        void InitGallery(GaleriasData.GalleryData gallery)
        {
            BackBtn.SetActive(false);
            Color color = Data.Instance.palettesManager.GetColor(gallery.colorUI);
            BackBtn.GetComponent<Image>().color = color;
            DoneBtn.GetComponent<Image>().color = color;
            ResetBtn.GetComponent<Image>().color = color;
            cam.backgroundColor = color;

            foreach (Image image in allColorizableSprites)
                image.color = color;

            color = Data.Instance.palettesManager.GetColor(gallery.colors[0]);
            SetBgColor(color);
            AudioManager.Instance.musicManager.Play("board");
            Invoke("Delayed", 0.5f);
        }
        void Delayed()
        {
            BackBtn.SetActive(true);
        }
        public void ResetButton()
        {
            AudioManager.Instance.uiSfxManager.PlayNextScale("click");
            ResetBoard();
        }

        public void ResetBoard()
        {
            GetComponent<DeleteAllPopup>().Init();
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
                PalettesManager.colorNames[] names = Data.Instance.galeriasData.gallery.colors;
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

        public void ActivateUIButtons(bool enable)
        {
            ActivateTilde(enable);
            ResetBtn.SetActive(enable);
        }

        public void ActivateTilde(bool enable)
        {
            BackBtn.SetActive(!enable);
            DoneBtn.SetActive(enable);
            zoomBar.SetActive(enable);
        }

        public void ClickBgColorBtn(Color c)
        {
            AudioManager.Instance.uiSfxManager.PlayNextScale("click");
            SetBgColor(c);
            ActivateTilde(true);
        }

        public void SetBgColor(Color color)
        {
            cam.backgroundColor = color;
        }

        public void SaveWork()
        {
            AudioManager.Instance.uiSfxManager.Play("tilde", 0.4f);
            screenshot.TakeShot(Data.Instance.albumData.thumbSize, OnTakeShotDone);
        }

        public void OnTakeShotDone(Texture2D tex)
        {
            Data.Instance.albumData.SaveWork(tex);
        }

        public IEnumerator CreateGif(string id, System.Action callback)
        {
            yield return new WaitForSeconds(1);
            AlbumData.WorkData wd = Data.Instance.albumData.SetCurrentID(id);
            Data.Instance.galeriasData.SetGallery(wd.galleryID);
            OpenWork(wd);
            items.NextStepAnims(0, captureGifFramerate);
            Events.CaptureGif(id, callback);
        }
      
        public void LoadWork(string id)
        {
            items.DeleteAll();
            AlbumData.WorkData wd = Data.Instance.albumData.SetCurrentID(id);
            UIManager.Instance.gallerySelectorUI.SelectGallery(wd.galleryID);
            OpenWork(wd);
        }
        //public void LoadPakapakaWork(int id)
        //{
        //    AlbumData.WorkData wataToClonate = Data.Instance.albumData.pakapakaAlbum[id];
        //    print("LoadPakapakaWork " + id);
        //    AlbumData.WorkData wData = new AlbumData.WorkData();
        //    wData.bgColorName = wataToClonate.bgColorName;
        //    wData.galleryID = wataToClonate.galleryID;
        //    wData.id = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        //    wData.items = wataToClonate.items;
        //    Data.Instance.albumData.album.Add(wData);
        //    LoadWork(wData.id);
        //}
        [SerializeField] RenderTexture rt;
        public void GenerateThumb(AlbumData.WorkData wd, RawImage targetRawImage, System.Action OnReady)
        {
            items.DeleteAll();
            UIManager.Instance.gallerySelectorUI.SelectGallery(wd.galleryID);
            foreach (AlbumData.WorkData.SavedIData itemData in wd.items)
            {
                ItemData newItem = Instantiate(Resources.Load<ItemData>("galerias/" + Data.Instance.galeriasData.gallery.id + "/item_" + itemData.id));
                // Debug.Log("ID" + itemData.id + ":" + itemData.position);
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
                items.SetItemInScene(itemInScene);

                newItem.SetTransformByData();
            }
            StartCoroutine(GenerateThumbForRawImage(wd, targetRawImage, OnReady));
        }

        public Vector2Int thumbSize = new Vector2Int(128, 128);


        public IEnumerator GenerateThumbForRawImage(AlbumData.WorkData wd, RawImage targetRawImage, System.Action OnReady)
        {
            // 1️⃣ Crear RenderTexture temporal
            RenderTexture rt = new RenderTexture(thumbSize.x, thumbSize.y, 24, RenderTextureFormat.ARGB32);
            rt.Create();

            cam.targetTexture = rt;

            yield return new WaitForEndOfFrame();

            // 2️⃣ Capturar frame en un Texture2D independiente
            Texture2D tex = new Texture2D(thumbSize.x, thumbSize.y, TextureFormat.RGBA32, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, thumbSize.x, thumbSize.y), 0, 0);
            tex.Apply(); // ✅ Ahora tex es una imagen independiente en memoria

            // 3️⃣ Asignamos la textura al RawImage
            targetRawImage.texture = tex;

            // 4️⃣ DESCONECTAR la cámara del RenderTexture
            cam.targetTexture = null;
            RenderTexture.active = null;

            // 5️⃣ Liberar SOLO el RenderTexture (¡NO destruir el Texture2D!)
            rt.Release();
            Destroy(rt); 
            yield return new WaitForEndOfFrame();
            OnReady();
        }








        void OpenWork(AlbumData.WorkData wd)
        {
            foreach (AlbumData.WorkData.SavedIData itemData in wd.items)
            {
                ItemData newItem = Instantiate(Resources.Load<ItemData>("galerias/" + Data.Instance.galeriasData.gallery.id + "/item_" + itemData.id));
                // Debug.Log("ID" + itemData.id + ":" + itemData.position);
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
                items.SetItemInScene(itemInScene);

                newItem.SetTransformByData();

                if (newItem.anim != AnimationsManager.anim.NONE)
                {
                    AnimationsManager.AnimData animData = Data.Instance.animationsManager.GetAnimByName(newItem.anim);
                    Events.AnimateItem(animData);
                }
                //itemData.Init();
                //Events.OnNewItem(iData);
            }
            //Events.ActivateUIButtons(true);
            cam.backgroundColor = Data.Instance.palettesManager.GetColor(wd.bgColorName);
        }

    }

}