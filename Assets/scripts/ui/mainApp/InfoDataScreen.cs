using BoardItems.BoardData;
using System;
using UI.MainApp.Home;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp
{
    public class InfoDataScreen : MonoBehaviour
    {
        public GameObject panel;
        public Image workImage;
        [SerializeField] TagsEditor tagsEditor;
        [SerializeField] ShareBtn shareBtn;
        string id;
        bool isPublic;
        MetadataTypes metadataType;
        void Start()
        {
            panel.SetActive(false);            
        }
        public void Init( string _id, MetadataTypes type, Sprite s)
        {   
            panel.SetActive(true);
            id = _id;
            metadataType = type;
            bool isPublic = false;
            if (type == MetadataTypes.stories) {
                tagsEditor.gameObject.SetActive(false);
                isPublic = Data.Instance.scenesData.GetMeta(id).isPublic;
            } else {
                CharacterMetaData md = type == MetadataTypes.so ? Data.Instance.sObjectsData.GetMeta(id) : Data.Instance.charactersData.GetMeta(id);
                isPublic = md.isPublic;
                tagsEditor.gameObject.SetActive(true);
                if (md.tags.Count > 0) // solo muestra 1 por ahora.
                    tagsEditor.Init(md.tags, Data.Instance.tagsManager.Tags);
            }
            shareBtn.Init(isPublic,OnSharedChanged); 
            workImage.sprite = s;            
        }
        void OnSharedChanged(bool isPublic)
        {
            this.isPublic = isPublic;
        }

        public void Save()
        {
            Events.OnLoadingParent(null, OnLoadingDone);
        } 
        void OnLoadingDone()
        {            
            Events.OnLoading(true);
            if(metadataType == MetadataTypes.so) 
                Data.Instance.sObjectsData.SaveInfo(id, isPublic, tagsEditor.GetSelectedTags(), OnDone);
            else if (metadataType == MetadataTypes.characters)
                Data.Instance.charactersData.SaveInfo(id, isPublic, tagsEditor.GetSelectedTags(), OnDone);
        }

        private void OnDone(bool success, string text)
        {
            Events.OnLoading(false);
            Events.OnPopupTopSignalText(text);
            Close();
        }

        public void Close()
        {            
            panel.SetActive(false);
        }

    }
}