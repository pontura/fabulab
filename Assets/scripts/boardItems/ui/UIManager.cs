using UnityEngine;

namespace BoardItems.UI
{
    public class UIManager : MonoBehaviour
    {
        static UIManager mInstance = null;
        public MainMenuUI mainMenuUI;
        public BoardUI boardUI;
        public GallerySelectorUI gallerySelectorUI;
        public AlbumUI albumUI;
        public WorkDetailUI workDetailUI;
        public ZoomsManager zoomManager;

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
            AudioManager.Instance.musicManager.StopLoop();
            AudioManager.Instance.musicManager.Play("intro");
            UIManager.Instance.mainMenuUI.ShowMainMenu(true);
            //AudioManager.Instance.musicManager.Play("main");
        }
    }

}