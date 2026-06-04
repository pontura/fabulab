using System;
using BoardItems.BoardData;
using UI.MainApp.Home;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp
{
    public class InfoDataScreen : MonoBehaviour
    {
        public GameObject panel;
        public Image workImage;
        [SerializeField] TagsSelector tagsSelector;
        [SerializeField] ShareBtn shareBtn;
        string id;
        string tagValue;
        bool isPublic;
        void Start()
        {
            panel.SetActive(false);            
        }
        public void Init( string _id, Sprite s)
        {   
            tagValue = Data.Instance.tagsManager.Tags[0].name;
            panel.SetActive(true);
            id = _id;
            PropMetaData sod = Data.Instance.sObjectsData.GetMeta(id);            
            shareBtn.Init(sod.isPublic,OnSharedChanged); 
            workImage.sprite = s;
            tagsSelector.Init(OnTagSelected);

            if(sod.tags.Count>0) // solo muestra 1 por ahora.
                tagsSelector.SetTag(sod.tags[0]);
        }
        void OnSharedChanged(bool isPublic)
        {
            this.isPublic = isPublic;
        }
         private void OnTagSelected(string value)
        {
            this.tagValue = value;
            print("Tag seleccionado: " + value);
        }

        public void Save()
        {
            Events.OnLoadingParent(null, OnLoadingDone);
        } 
        void OnLoadingDone()
        {            
            Events.OnLoading(true);
            Data.Instance.sObjectsData.SaveInfo(id, isPublic, tagValue, OnDone);  
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