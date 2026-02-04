using BoardItems;
using BoardItems.UI;
using UI.MainApp;
using UnityEngine;

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
            Creation
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
        public void LoadWork(string id)
        {
            boardUI.LoadWork(id);
            Events.ShowScreen(UIManager.screenType.Creation);
        }
        public void ShowGallerySelector()
        {
            Events.ShowScreen(UIManager.screenType.Galleries);
        }
        public void InitGallery(GaleriasData.GalleryData gd, bool a, System.Action s)
        {
            Events.InitGallery(gd, a, s);
            Events.ShowScreen(UIManager.screenType.Creation);
        }
    }

}