using System.Collections.Generic;
using UI;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.StoryMaker.DB;
using BoardItems.BoardData;

namespace BoardItems
{
    public class SObjectsData : MonoBehaviour {
        public Vector2Int thumbSize;

        public List<CharacterData> data;
        public List<CharacterMetaData> metaData;

        [SerializeField] string currentID;

        CharacterData currentSO;
        Dictionary<string, ServerPartMetaData> serverPartsMetaData;

       
        private void Start() {
            FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;          
        }       

        private void OnDestroy() {
            FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
        }

        void OnTokenUpdated() {
            Debug.Log("#OnTokenUpdated");
            if (Data.Instance.userData.IsLogged()) {
                CancelInvoke();
                LoadSOMetadataFromServer();                
            } else
                Invoke("OnTokenUpdated", 1);
        }
        void LoadSOMetadataFromServer() {
            Debug.Log("#Load SO MetadataFromServer");
            metaData = new List<CharacterMetaData>();
            FirebaseStoryMakerDBManager.Instance.LoadMetadataFromServer("so", OnLoadSODataFromServer);
        }        

        public void SaveSO(Texture2D tex)
        {
            CharacterData wd;
            if (currentID == "" || currentID == null)
            {
                wd = new CharacterData();
                wd.id = "";
            }
            else
                wd = GetSO(currentID);

            wd.thumb = tex;
            wd.items = new List<SavedIData>();
            int totalItems = UIManager.Instance.boardUI.items.all.Count;
         
            int i = 0;
            while (i < totalItems)
            {
                ItemInScene iInScene = UIManager.Instance.boardUI.items.all[0];
                int newPartID = (int)iInScene.data.part;
             
                SavedIData sd = new SavedIData();
                sd.part = newPartID;
                sd.id = iInScene.data.id;
                sd.position = iInScene.data.position;
                sd.rotation = iInScene.data.rotation;
                sd.scale = iInScene.data.scale;
                sd.anim = iInScene.data.anim;
                sd.color = iInScene.data.colorName;
                sd.galleryID = iInScene.data.galleryID;
                wd.items.Add(sd);
                UIManager.Instance.boardUI.items.Delete(iInScene);
                i++;
            }
            currentSO = wd;

            string type = "so";
            if (wd.id == "") {
                data.Add(wd);
                FirebaseStoryMakerDBManager.Instance.SaveToServer("so", wd.GetServerData(), OnSavedToServer);
            } 
            else {
                FirebaseStoryMakerDBManager.Instance.UpdateDataToServer("so", wd.id, wd.GetServerData(), OnSavedToServer);
            } 
        }
        void OnSavedToServer(bool succes, string id)
        {
            currentSO.id = id;
            currentID = id;

            ServerCharacterMetaData swmd = new ServerCharacterMetaData();
            swmd.thumb = System.Convert.ToBase64String(currentSO.thumb.EncodeToPNG());
            swmd.userID = Data.Instance.userData.userDataInDatabase.uid;
            swmd.AddCreator(Data.Instance.userData.userDataInDatabase.uid); 
            FirebaseStoryMakerDBManager.Instance.SaveMetadataToServer("so", currentID, swmd);

            OpenSODetail(currentSO);
        }
        void OpenSODetail(CharacterData wd)
        {
            Debug.Log("Open SO Detal !!");         
        }
        public void OnLoadSODataFromServer(List<CharacterMetaData> sfds)
        {
            Debug.Log("OnLoadSODataFromServer !!");
            metaData = sfds;
            data = new();
            //  metaData = charactersMetaData.FindAll(x => x.userID == Data.Instance.userData.userDataInDatabase.uid);
            // userCharacters = new();
            FirebaseStoryMakerDBManager.Instance.LoadUserAssetsFromServer("so", LoadAssetsFromServer);
        }

        void LoadAssetsFromServer(Dictionary<string, CharacterServerData> d)
        {
            foreach (KeyValuePair<string, CharacterServerData> e in d)
            {
                Debug.Log("#LoadCharactersFromServer " + e.Key + ": " + e.Value);
                CharacterData wd = new CharacterData();
                wd.id = e.Key;
                Debug.Log("#Aca1: " + e.Value.items.Count);
                wd.LoadServerData(e.Value);
                Debug.Log("#Aca2");
                wd.thumb = metaData.Find(x => x.id == wd.id)?.thumb;
                data.Add(wd);
            }
        }
        
        public string GetCurrent()
        {
            return currentID;
        }
        public CharacterData GetSO(string id)
        {
            return data.Find(x => x.id == id);
        }

        public CharacterData SetCurrentID(string id)
        {
            currentID = id;
            return data.Find(x => x.id == id);
        }

        public void ResetCurrentID()
        {
            currentID = "";
        }

        public void DeleteCharacter(string id)
        {
            //CharacterData wd = characters.Find(x => x.id == id);
            //characters.Remove(wd);
           // PlayerPrefs.DeleteKey("Work_" + id);
            //  PlayerPrefs.DeleteKey("PkpkShared_" + id);
           // PersistWorksIds();
        }


        public bool HasAnims(string id)
        {
            return false;
            //CharacterData wd = userCharacters.Find(x => x.id == id);
            //bool hasAnims = false;
            //foreach (SavedIData item in wd.items)
            //{
            //    if (item.anim != AnimationsManager.anim.NONE)
            //    {
            //        hasAnims = true;
            //        return hasAnims;
            //    }
            //}
            //return hasAnims;
        }


        void AddPart(CharacterData wd)
        {
            data.Add(wd);
        }
        public List<CharacterData> GetPreset()
        {
            return data;
        }
    }

}
