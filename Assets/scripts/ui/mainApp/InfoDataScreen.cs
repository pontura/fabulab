using BoardItems;
using BoardItems.BoardData;
using Firebase.Analytics;
using System.Collections.Generic;
using UI.MainApp.Home;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp
{
    public class InfoDataScreen : MonoBehaviour
    {
        public GameObject panel;
        public Image workImage;
        [SerializeField] TagsEditor tagsEditor;
        [SerializeField] ShareBtn shareBtn;
        [SerializeField] TMPro.TMP_InputField nameField;
        string id;
        [SerializeField] bool isPublic;
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
            isPublic = false;
            if (type == MetadataTypes.stories) {
                tagsEditor.gameObject.SetActive(false);
                FilmDataFabulab filmDataFabulab = Data.Instance.scenesData.GetMeta(id);
                isPublic = filmDataFabulab.isPublic;
                nameField.text = filmDataFabulab.name;
            } else {
                CharacterMetaData md = type == MetadataTypes.so ? Data.Instance.sObjectsData.GetMeta(id) : Data.Instance.charactersData.GetMeta(id);
                isPublic = md.isPublic;
                tagsEditor.gameObject.SetActive(true);
                nameField.text = md.name;
                tagsEditor.Init(md.tags);
            }
            shareBtn.Init(isPublic,OnSharedChanged); 
            workImage.sprite = s;            
        }
        void OnSharedChanged(bool isPublic)
        {
            this.isPublic = isPublic;
        }

        void Save()
        {
            Events.OnLoadingParent(null, OnLoadingDone);
        } 
        void OnLoadingDone()
        {            
            Events.OnLoading(true);
            if (metadataType == MetadataTypes.so) {
                if (ItemHasChanged())
                    Data.Instance.sObjectsData.SaveInfo(id, isPublic, nameField.text, tagsEditor.GetSelectedTags(), OnDone);
                else
                    OnDone(true,"");
            } else if (metadataType == MetadataTypes.characters) {
                if (ItemHasChanged())
                    Data.Instance.charactersData.SaveInfo(id, isPublic, nameField.text, tagsEditor.GetSelectedTags(), OnDone);
                else
                    OnDone(true, "");
            } else if (metadataType == MetadataTypes.stories) {
                if(StoryHasChanged())
                    Data.Instance.scenesData.SaveInfo(id, isPublic, tagsEditor.GetSelectedTags(), OnDone);
                else
                    OnDone(true, "");
            }
        }

        bool StoryHasChanged() {
            FilmDataFabulab filmDataFabulab = Data.Instance.scenesData.GetMeta(id);
            return isPublic != filmDataFabulab.isPublic || nameField.text != filmDataFabulab.name;
        }
        bool ItemHasChanged() {
            CharacterMetaData md = metadataType == MetadataTypes.so ? Data.Instance.sObjectsData.GetMeta(id) : Data.Instance.charactersData.GetMeta(id);
            return isPublic != md.isPublic || nameField.text != md.name || !CompareTags(md.tags, tagsEditor.GetSelectedTags());
        }

        bool CompareTags(List<string> original, List<string> current) {
            
            if ((original == null || original.Count == 0) && current.Count == 0)
                return true;

            if (original == null && current.Count > 0)
                return false;

            return new HashSet<string>(original).SetEquals(current);
        }

        private void OnDone(bool success, string text)
        {
            Events.OnLoading(false);
            if(!string.IsNullOrEmpty(text))
                Events.OnPopupTopSignalText(text);
            panel.SetActive(false);
        }

        public void Close()
        {
            Save();
        }

        public void OpenWork() {
            string eventName = "";
            string parameterName = "";
            if (metadataType == MetadataTypes.stories) {
                Events.OnLoadingParent(null, LoadingStoryDone);
                eventName = "story_edit";
                parameterName = "story_id";
            } else if (metadataType == MetadataTypes.characters) {
                UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);
                eventName = "character_edit";
                parameterName = "item_id";
                Close();
            } else if (metadataType == MetadataTypes.so) {
                UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
                eventName = "object_edit";
                parameterName = "item_id";
                Close();
            }
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                eventName,
                new Parameter(parameterName, id)
            );
        }

        public void DuplicateWork() {
            string eventName = "";
            string parameterName = "";
            if (metadataType == MetadataTypes.stories) {
                Events.OnLoadingParent(null, () => {
                    LoadingStoryDone();
                    ScenesManagerFabulab.Instance.currentFDataID = "";
                });
                eventName = "story_duplicate";
                parameterName = "story_id";
            } else if (metadataType == MetadataTypes.characters) {
                UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);
                Data.Instance.charactersData.SetCurrentID("");
                eventName = "character_duplicate";
                parameterName = "item_id";
                Close();
            } else if (metadataType == MetadataTypes.so) {
                UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
                Data.Instance.sObjectsData.SetCurrentID("");
                eventName = "object_duplicate";
                parameterName = "item_id";
                Close();
            }
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                eventName,
                new Parameter(parameterName, id)
            );
        }

        void LoadingStoryDone() {
            Events.OnLoading(true);
            Data.Instance.scenesData.LoadUserFilm(id);
            UIManager.Instance.boardUI.SetEditingType(BoardUI.editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            Invoke(nameof(SetUserStoryEditionState), Time.deltaTime * 2);            
        }
        void SetUserStoryEditionState() {
            StoryMakerEvents.EnableStoryEdition(true);
            Close();
        }

        public void DeleteWork() {
            if (!string.IsNullOrEmpty(id)) {
                print("Delete ID: " + id);
                Events.OnConfirm($"¿Confirmás que querés borrar este item?", "SI", "NO", OnConfirm);
            }
        }
        protected virtual void OnConfirm(bool ok) {
            if (ok) {
                if (metadataType == MetadataTypes.stories)
                    FirebaseStoryMakerDBManager.Instance.DeleteFilm(Data.Instance.scenesData.userFilmsData.Find(x => x.id == id), OnDeleted);
                else
                    FirebaseStoryMakerDBManager.Instance.DeletePart(metadataType.ToString(), id, OnDeleted, Data.Instance.userData.userDataInDatabase.uid);
            }
        }
        protected virtual void OnDeleted(string id) {
            if (metadataType == MetadataTypes.characters) {
                Data.Instance.charactersData.RemoveCharacter(id);
                Events.OnCharacterMetadataRemoved(id);
            } else if (metadataType == MetadataTypes.so) {
                Data.Instance.sObjectsData.RemoveSO(id);
                Events.OnPropMetadataRemoved(id);
            }
            panel.SetActive(false);
        }

    }
}