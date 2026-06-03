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

        protected MetadataTypes metadataType;
        string itemUserId;

        public string Id { get; private set; }

        public override void Init(Sprite sprite) {
            base.Init(sprite);
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
            loading.SetActive(false);
        }

        public void Init(SOPartData cd)
        {
            //thumb.sprite = cd.GetSprite();
            Id = cd.id;
            deleteBtn.gameObject.SetActive(Data.Instance.userData.isAdmin);
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
