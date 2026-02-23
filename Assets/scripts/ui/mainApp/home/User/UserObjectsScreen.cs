using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home.User
{
    public class UserObjectsScreen : MonoBehaviour
    {
        public CharacterSelectorBtn workBtn_prefab;
        public Transform worksContainer;

        int artID = 0;
        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if(isOn)
            {
                Init();
            }
        }

        public void Init()
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
            print("SO LoadNext " + artID);
            print("SO count:  " + Data.Instance.sObjectsData.data.Count);
            // if (artID >= Data.Instance.characters.Count) return;
            foreach (CharacterData cd in Data.Instance.sObjectsData.data)
            {
                CharacterSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                print("go " + go);
                go.Init(cd);
                //RawImage rm = go.GetComponentInChildren<RawImage>();
               // UIManager.Instance.boardUI.GenerateThumb(wd, rm, LoadNext);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }
            //CharacterData wd = Data.Instance.albumData.characters[artID];
            //artID++;
            //if (wd.thumb != null)
            //{
            //    //if (Data.Instance.galeriasData.ExistGallery(wd.galleryID))
            //    //{
            //        GameObject go = Instantiate(workBtn_prefab, worksContainer);
            //        print("go " + go);
            //        //RawImage rm = go.GetComponentInChildren<RawImage>();
            //        //UIManager.Instance.boardUI.GenerateThumb(wd, rm, LoadNext);
            //        go.GetComponent<Button>().onClick.AddListener(() => OpenWork(wd.id));
            //    //}
            //}
        }
        public void DuplicateWork(int PakaPakaObjectID)
        {
          //  UIManager.Instance.boardUI.LoadPakapakaWork(PakaPakaObjectID);
           // album.SetActive(false);
        }

        public void OpenWork(string id)
        {
            UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
        }

        public void New()
        {
            print("New Character");
            UIManager.Instance.NewCharacter();
        }
    }

}