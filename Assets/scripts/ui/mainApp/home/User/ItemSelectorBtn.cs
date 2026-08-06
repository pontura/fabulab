using BoardItems.BoardData;
using BoardItems.Characters;
using System;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class ItemSelectorBtn : SimpleButton
    {
        [SerializeField] CreatorsList creatorList;
        [SerializeField] protected Button deleteBtn;
        [SerializeField] protected GameObject loading;
        [SerializeField] protected ToggleButton infoBtn;
        [SerializeField] protected LikeToggle likeBtn;
        public string Id { get; private set; }

        protected MetadataTypes metadataType;
        string itemUserId;


        public void Init(Sprite sprite, System.Action<string> OnClicked) {
            base.Init(sprite);
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
            loading.SetActive(false);
            infoBtn.gameObject.SetActive(false);
            likeBtn.gameObject.SetActive(false);            
            AddOnClick(OnClicked);
            SetLikeButton();
        }
        public void Init(SOPartData cd, System.Action<string> OnClicked, bool storyEditing)
        {
            Id = cd.id;
            /*if(userView)
            {
                PropMetaData meta = Data.Instance.sObjectsData.GetMeta(cd.id);
                bool isPublic = meta.isPublic;
                print("ItemSelectorBtn id: " + cd.id + " isPublic: " + isPublic);   
                infoBtn.gameObject.SetActive(true);
                infoBtn.Init(OnInfoClicked, isPublic);
            }  else            */
            infoBtn.gameObject.SetActive(false);
           // likeBtn.gameObject.SetActive(true);
            likeBtn.Init(OnLikeToggle, Data.Instance.userData.isLiked(Id));
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
            AddOnClick(OnClicked);
            SetLikeButton(storyEditing);
        }        
        protected void OnInfoClicked(bool isPublic)
        {
            AudioManager.Instance.uiSfxManager.PlayTransp("click", 2);
            UIManager.Instance.infoDataScreen.Init(Id, metadataType, thumb.sprite);
            print($"Info clicked id: {Id} isPublic: " + isPublic);
        }
        protected void OnLikeToggle(bool addLike) {
            AudioManager.Instance.uiSfxManager.PlayTransp("click", 2);
            Data.Instance.userData.OnLikeUpdate(metadataType,Id,addLike);
            print($"Like clicked id: {Id} adding: " + addLike);
        }
        public void Init(CharacterMetaData cd, MetadataTypes type, System.Action<string> OnClicked, bool userView = false) {
            Init(cd, type);
            print("SHOW userView " + userView);
            if (userView) {
                UpdatePublicState();
            } else {
                infoBtn.gameObject.SetActive(false); 
                likeBtn.Init(OnLikeToggle, cd.likes, Data.Instance.userData.isLiked(Id));
            }    
            SetLikeButton();   
            AddOnClick(OnClicked);
        }

        public void Init(CharacterMetaData cd, MetadataTypes type) {
            //thumb.sprite = cd.GetSprite();
            creatorList.Init(cd.creators);
            Id = cd.id;
            metadataType = type;
            itemUserId = cd.userID;
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
            likeBtn.gameObject.SetActive(false);
        }
        virtual public void Init(string id, Sprite sprite) {
            thumb.sprite = sprite;
            Id = id;
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
            infoBtn.gameObject.SetActive(false);            
            loading.SetActive(false);
        }
        public void SetLikeButton(bool storyEditing = false)
        {        
            print("SetLikeButton " + storyEditing);    
            likeBtn.gameObject.SetActive(!storyEditing);            
        }

        public void AddOnClick(System.Action<string> OnClicked)
        {
            transform.GetComponentInChildren<Button>().onClick.AddListener(() => {
                AudioManager.Instance.uiSfxManager.PlayTransp("click", 5);
                OnClicked?.Invoke(Id);
            });            
        }
      
        public void UpdatePublicState() {
            bool isPublic = false;
            if (metadataType == MetadataTypes.so)
                isPublic = Data.Instance.sObjectsData.GetMeta(Id).isPublic;
            else if (metadataType == MetadataTypes.characters)
                isPublic = Data.Instance.charactersData.GetMeta(Id).isPublic;
            else if (metadataType == MetadataTypes.stories)
                isPublic = Data.Instance.scenesData.GetMeta(Id).isPublic;

            print("ItemSelectorBtn id: " + Id + " isPublic: " + isPublic);
            infoBtn.gameObject.SetActive(true);
            infoBtn.Init(OnInfoClicked, isPublic);
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
                Data.Instance.sObjectsData.RemoveSO(id);
                Events.OnPropMetadataRemoved(id);
            }
            Destroy(gameObject);
        }

        void OnLikeClicked() {
            AudioManager.Instance.uiSfxManager.PlayTransp("click", 2);
            
        }       
    }
}
