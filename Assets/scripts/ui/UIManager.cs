using BoardItems;
using BoardItems.Characters;
using System.Collections.Generic;
using UI.MainApp;
using UnityEngine;
using static BoardItems.AlbumData;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public GameObject backBtn;
        static UIManager mInstance = null;
        public CreatePanel create;
        public BoardUI boardUI;
        public GallerySelectorUI gallerySelectorUI;
        public WorkDetailUI workDetailUI;
        public ZoomsManager zoomManager;
        CharacterEdition characterEdition;
        [SerializeField] ConfirmationScreen confirmationScreen;

        public enum screenType
        {
            Home,
            Create,
            Albums,
            Galleries,
            Creation_Character,
            WorkDetail,
            UserScreen,
            Creation_Objects
        }

        public static UIManager Instance
        {
            get
            {
                return mInstance;
            }
        }
        List<screenType> backToScreen;
        void Awake()
        {
            confirmationScreen.Init();
            backToScreen = new List<screenType>();
            zoomManager = GetComponent<ZoomsManager>();
            characterEdition = GetComponent<CharacterEdition>();
            if (!mInstance)
                mInstance = this;
        }
        private void Start()
        {
            Events.ShowScreen += OnShowScreen;
            Init();
            Invoke("InitGalleryDelayed", 0.1f);
        }
        private void OnDestroy()
        {
            Events.ShowScreen -= OnShowScreen;
        }
        private void OnShowScreen(screenType type)
        {
            backToScreen.Add(type);
            switch(type)
            {
                case screenType.Home:
                    backBtn.SetActive(false);
                    break;
                default:
                    backBtn.SetActive(true);
                    break;
            }
        }
        void InitGalleryDelayed() // to-do inicia los items:
        {
            GaleriasData.GalleryData gd = Data.Instance.galeriasData.GetGallery(1);
           // InitGallery(gd, true, null);
            Events.InitGallery(gd, true, null);
        }
        void Init()
        {
            Home();
        }
        public void Home()
        {
            Events.ShowScreen(UIManager.screenType.Home);
        }
        public void Create()
        {
            Events.ShowScreen(UIManager.screenType.Create);
        }
        public void CreateSelected(int id)
        {
            NewCharacter();//Albums();
        }
        public void Albums()
        {
            Events.ShowScreen(UIManager.screenType.Albums);
        }       
        public void NewCharacter()
        {
            print("NewCharacter");
            Data.Instance.albumData.ResetCurrentID();
            Events.OnNewCharacter();
            GaleriasData.GalleryData gd = Data.Instance.galeriasData.GetGallery(1);
            InitGallery(gd, true, null);
            Events.EmptyCharacterItems();
        }
        public void LoadWork(string id)
        {
            boardUI.LoadWork(id);
            Events.ShowScreen(UIManager.screenType.Creation_Character);
        }
        public void InitGallery(GaleriasData.GalleryData gd, bool a, System.Action s)
        {
            Events.InitGallery(gd, a, s);
            Events.ShowScreen(UIManager.screenType.Creation_Character);
        }
        public void Back()
        {
            if (backToScreen[backToScreen.Count - 1] == screenType.Creation_Character && characterEdition.ChangesMade())
            {
                Events.OnConfirm("All changes will be lost", "Confirm and exit", "Cancel", ExitConfirmed);
            } else {
                SetBack();
            }
        }
        void SetBack()
        {
            if (backToScreen.Count < 3)
                Events.ShowScreen(screenType.Home);
            else
                Events.ShowScreen(backToScreen[backToScreen.Count - 2]);
            backToScreen.RemoveAt(backToScreen.Count - 1);
            backToScreen.RemoveAt(backToScreen.Count - 1);
        }
        void ExitConfirmed(bool exit)
        {
            if(exit)
                SetBack();
        }
        public void ShowWorkDetail(AlbumData.CharacterData wd)
        {
            Sprite sprite = Sprite.Create(wd.thumb, new Rect(0, 0, wd.thumb.width, wd.thumb.height), Vector2.zero);
            Events.ShowScreen(UIManager.screenType.WorkDetail);
            workDetailUI.ShowWorkDetail(wd.id, sprite, true);
            Events.ResetItems();
        }
    }

}