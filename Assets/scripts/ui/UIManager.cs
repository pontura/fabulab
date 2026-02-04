using BoardItems;
using UI.MainApp;
using UnityEngine;
using static BoardItems.AlbumData;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        static UIManager mInstance = null;
        public HomePage home;
        public Header header;
        public CreatePanel create;
        public BoardUI boardUI;
        public GallerySelectorUI gallerySelectorUI;
        public AlbumUI albumUI;
        public WorkDetailUI workDetailUI;
        public ZoomsManager zoomManager;
        public enum screenType
        {
            Home,
            Create,
            Albums,
            Galleries,
            Creation,
            WorkDetail
        }

        public static UIManager Instance
        {
            get
            {
                return mInstance;
            }
        }
        void Awake()
        {
            zoomManager = GetComponent<ZoomsManager>();
            if (!mInstance)
                mInstance = this;
        }
        private void Start()
        {
            Init();
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
            Albums();
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
            Events.ShowScreen(UIManager.screenType.Creation);
        }
        public void InitGallery(GaleriasData.GalleryData gd, bool a, System.Action s)
        {
            Events.InitGallery(gd, a, s);
            Events.ShowScreen(UIManager.screenType.Creation);
        }
        public void Back()
        {
            Home();
        }
        public void ShowWorkDetail(CharacterData wd)
        {
            Sprite sprite = Sprite.Create(wd.thumb, new Rect(0, 0, wd.thumb.width, wd.thumb.height), Vector2.zero);
            Events.ShowScreen(UIManager.screenType.WorkDetail);
            workDetailUI.ShowWorkDetail(wd.id, sprite, true);
            Events.ResetItems();
        }
    }

}