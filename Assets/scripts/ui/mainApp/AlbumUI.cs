using BoardItems;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp
{
    public class AlbumUI : MainScreen
    {
        public GameObject workBtn_prefab;
        public Transform worksContainer;

        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Albums:
                    Show(true);
                    break;
                default:
                    Show(false);
                    break;
            }
        }

        public override void OnInit() { 
            Init();
        }
        int artID = 0;
        void Init()
        {
            artID = 0;

            foreach (Transform child in worksContainer)
            {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }

            LoadNext();
        }
        
        void LoadNext()
        {
            print("LoadNext " + artID);
            if (artID >= Data.Instance.albumData.characters.Count) return;
            AlbumData.CharacterData wd = Data.Instance.albumData.characters[artID];
            artID++;
            if (wd.thumb != null)
            {
                //if (Data.Instance.galeriasData.ExistGallery(wd.galleryID))
                //{
                    GameObject go = Instantiate(workBtn_prefab, worksContainer);
                    print("go " + go);
                    RawImage rm = go.GetComponentInChildren<RawImage>();
                    UIManager.Instance.boardUI.GenerateThumb(wd, rm, LoadNext);
                    go.GetComponent<Button>().onClick.AddListener(() => OpenWork(wd.id));
                //}
            }
        }
        public void DuplicateWork(int PakaPakaObjectID)
        {
          //  UIManager.Instance.boardUI.LoadPakapakaWork(PakaPakaObjectID);
           // album.SetActive(false);
        }

        public void OpenWork(string id) { 
            UIManager.Instance.LoadWork(id);
        }

        public void New()
        {
            print("New Character");
            UIManager.Instance.NewCharacter();
        }
    }

}