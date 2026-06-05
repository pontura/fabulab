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
        [SerializeField] protected Button deleteBtn;
        [SerializeField] protected GameObject loading;
        [SerializeField] protected ToggleButton infoBtn;
        public string Id { get; private set; }

        protected MetadataTypes metadataType;
        string itemUserId;


        public void Init(Sprite sprite, System.Action<string> OnClicked) {
            base.Init(sprite);
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
            loading.SetActive(false);
            infoBtn.gameObject.SetActive(false);
            AddOnClick(OnClicked);
        }
        public void Init(SOPartData cd, System.Action<string> OnClicked, bool userView = false)
        {
            Id = cd.id;
            if(userView)
            {
                PropMetaData meta = Data.Instance.sObjectsData.GetMeta(cd.id);
                bool isPublic = meta.isPublic;
                print("ItemSelectorBtn id: " + cd.id + " isPublic: " + isPublic);   
                infoBtn.gameObject.SetActive(true);
                infoBtn.Init(OnInfoClicked, isPublic);
            }  else            
                infoBtn.gameObject.SetActive(false);
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
            AddOnClick(OnClicked);
        }        
        void OnInfoClicked(bool isPublic)
        {
            UIManager.Instance.infoDataScreen.Init(Id, thumb.sprite);
            print("Info clicked isPublic: " + isPublic);
        }
        public void Init(CharacterMetaData cd, MetadataTypes type, System.Action<string> OnClicked, bool userView = false) {
            print("SHOW userView " + userView);
            if(userView)
            {
                bool isPublic = false;
                if (type==MetadataTypes.so)
                    isPublic = Data.Instance.sObjectsData.GetMeta(cd.id).isPublic;
                else if(type==MetadataTypes.characters)
                    isPublic = Data.Instance.charactersData.GetMeta(cd.id).isPublic;
                print("ItemSelectorBtn id: " + cd.id + " isPublic: " + isPublic);  
                infoBtn.gameObject.SetActive(true);
                infoBtn.Init(OnInfoClicked, isPublic);
            } else            
                infoBtn.gameObject.SetActive(false);
                        
            AddOnClick(OnClicked);
            Init(cd, type);
        }

        public void Init(CharacterMetaData cd, MetadataTypes type) {
            //thumb.sprite = cd.GetSprite();
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

        public void AddOnClick(System.Action<string> OnClicked)
        {
            transform.GetComponentInChildren<Button>().onClick.AddListener(() => OnClicked?.Invoke(Id));            
        }
      

        public void SetSprite(Texture2D tex) {
            thumb.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            loading.SetActive(false);
        }

        public virtual void Delete() {
            if (Id != null && Id != "") {
                print("Delete ID: " + Id);
                Events.OnConfirm($"�Confirm�s que quer�s borrar id: {Id}?", "SI", "NO", OnConfirm);
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
                Data.Instance.sObjectsData.RemoveSO(id);
                Events.OnPropMetadataRemoved(id);
            }
            Destroy(gameObject);
        }
    }
}
