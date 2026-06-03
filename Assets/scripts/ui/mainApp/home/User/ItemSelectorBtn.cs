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
        {
            print("SHOW");
            //thumb.sprite = cd.GetSprite();
            Id = cd.id;
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
            creatorList.Init(cd.creators);
            Id = cd.id;
        }
        public void Init(string id, Sprite sprite) {
            thumb.sprite = sprite;
            Id = id;
        }

        public void SetSprite(Texture2D tex) {
            thumb.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }
    }
}
