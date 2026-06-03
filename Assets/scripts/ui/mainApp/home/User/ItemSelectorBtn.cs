using System;
using BoardItems.BoardData;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.DB;

namespace UI.MainApp.Home.User
{
    public class ItemSelectorBtn : SimpleButton
    {
        [SerializeField] CreatorsList creatorList;

        [SerializeField] ShareBtn shareBtn;

        public string Id { get; private set; }
        public void Init(SOPartData cd, System.Action<string> OnClicked)

        [SerializeField] protected Button deleteBtn;
        [SerializeField] protected GameObject loading;

        protected MetadataTypes metadataType;
        string itemUserId;

        public string Id { get; private set; }

        public override void Init(Sprite sprite) {
            base.Init(sprite);
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
            loading.SetActive(false);
        }

        public void Init(SOPartData cd)
//>>>>>>> 15ff1f4cc7177252b94f3167a54e253a1455899f
//        {
 //           print("SHOW");
 //           //thumb.sprite = cd.GetSprite();
            Id = cd.id;
//<<<<<<< HEAD
            bool isPublic = Data.Instance.sObjectsData.GetMeta(cd.id).isPublic;
            print("ItemSelectorBtn id: " + cd.id + " isPublic: " + isPublic);   
            shareBtn.Init(isPublic,OnSharedChanged);           
            transform.GetComponentInChildren<Button>().onClick.AddListener(() => OnClicked?.Invoke(Id));
        }        

        public void Init(CharacterMetaData cd, System.Action<string> OnClicked, bool userView = false) {
            print("SHOW userView " + userView);
            if(userView)
            {
                bool isPublic = Data.Instance.sObjectsData.GetMeta(cd.id).isPublic;
                print("ItemSelectorBtn id: " + cd.id + " isPublic: " + isPublic);  
                shareBtn.Init(isPublic,OnSharedChanged);   
                shareBtn.Show(true);
            } else            
                shareBtn.Show(false);

            creatorList.Init(cd.creators);
            Id = cd.id;
            transform.GetComponentInChildren<Button>().onClick.AddListener(() => OnClicked?.Invoke(Id));
        }

        public void OnSharedChanged(bool isPublic)
        {
            print("Shared changed isPublic: " + isPublic);
            Data.Instance.sObjectsData.ChangePublic(Id, isPublic);
        }

        public void Init(CharacterMetaData cd) {
//=======
//            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
//        }        
//
//        public void Init(CharacterMetaData cd, MetadataTypes type) {
 //           //thumb.sprite = cd.GetSprite();
//>>>>>>> 15ff1f4cc7177252b94f3167a54e253a1455899f
            creatorList.Init(cd.creators);
            Id = cd.id;
            metadataType = type;
            itemUserId = cd.userID;
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
        }
        virtual public void Init(string id, Sprite sprite) {
            thumb.sprite = sprite;
            Id = id;
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
        }

        public void SetSprite(Texture2D tex) {
            thumb.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            loading.SetActive(false);
        }

        public virtual void Delete() {
            if (Id != null && Id != "") {
                print("Delete ID: " + Id);
                Events.OnConfirm($"¿Confirmás que querés borrar id: {Id}?", "SI", "NO", OnConfirm);
            }
        }
        protected virtual void OnConfirm(bool ok) {
            if (ok) {
                //FirebaseStoryMakerDBManager.Instance.DeleteFilm(Data.Instance.scenesData.userFilmsData.Find(x => x.id == iD), OnDeleted);
                FirebaseStoryMakerDBManager.Instance.DeletePart(metadataType.ToString(),Id, OnDeleted, itemUserId);
            }
        }
        protected virtual void OnDeleted(string id) {
            if (metadataType == MetadataTypes.characters) {
                Data.Instance.charactersData.RemoveCharacter(id);
                Events.OnCharacterMetadataRemoved(id);
            }else if (metadataType == MetadataTypes.so) {
                Data.Instance.charactersData.RemoveCharacter(id);
                Events.OnPropMetadataRemoved(id);
            }
            Destroy(gameObject);
        }
    }
}
