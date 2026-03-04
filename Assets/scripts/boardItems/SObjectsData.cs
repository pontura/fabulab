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

        public List<SObjectData> data;
        public List<CharacterMetaData> metaData;

        public SObjectData.types Type { get { return this.currentType; } }
        public void SetType(SObjectData.types type) { this.currentType = type; }
        public List<SObjectData> GetDataByType(SObjectData.types type)
        {
           return data.FindAll(x => x.type == type);
        }

        [SerializeField] string currentID;
        [SerializeField] SObjectData.types currentType;

        SObjectData currentSO;
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
            SObjectData wd;
            if (currentID == "" || currentID == null)
            {
                wd = new SObjectData();
                wd.id = "";
            }
            else
                wd = GetSO(currentID);

            wd.bg = Data.Instance.palettesManager.bgColorName;
            wd.type = currentType;
            wd.thumb = tex;
            wd.items = new List<SavedIData>();

            List<ItemInScene> mirrors = new List<ItemInScene>();
            foreach (ItemInScene iInScene in UIManager.Instance.boardUI.items.all)
            {
                bool isMirror = false;
                foreach (ItemInScene m in mirrors)
                {
                    isMirror = (iInScene == m);
                }
                if (!isMirror)
                {
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
                    ItemInScene mirror = iInScene.GetMirror();
                    if (mirror != null)
                        mirrors.Add(mirror);
                }
                // UIManager.Instance.boardUI.items.Delete(iInScene);
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
        void OpenSODetail(SObjectData wd)
        {
            Debug.Log("Open SO Detal !!");         
        }
        public void OnLoadSODataFromServer(List<CharacterMetaData> sfds)
        {
            Debug.Log("OnLoadSODataFromServer !!");
            metaData = sfds;
            data = new();
            FirebaseStoryMakerDBManager.Instance.LoadUserAssetsFromServer("so", LoadAssetsFromServer);
        }

        void LoadAssetsFromServer(Dictionary<string, SObjectServerData> d)
        {
            foreach (KeyValuePair<string, SObjectServerData> e in d)
            {
                Debug.Log("#LoadCharactersFromServer " + e.Key + ": " + e.Value);
                SObjectData wd = new SObjectData();
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
        public SObjectData GetSO(string id)
        {
            return data.Find(x => x.id == id);
        }

        public SObjectData SetCurrentID(string id)
        {
            currentID = id;
            SObjectData o =  data.Find(x => x.id == id);
            if(o != null)
                currentType = o.type;
            return o;
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
        }


        void AddPart(SObjectData wd)
        {
            data.Add(wd);
        }
        public List<SObjectData> GetPreset()
        {
            return data;
        }
    }

}
