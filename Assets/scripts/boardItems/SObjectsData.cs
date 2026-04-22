using BoardItems.BoardData;
using System.Collections.Generic;
using UI;
using UI.MainApp;
using UnityEditor.U2D.Animation;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.StoryMaker.DB;

namespace BoardItems
{
    public class SObjectsData : MonoBehaviour
    {
        public Vector2Int thumbSize;

        public List<SObjectData> data;
        public List<SObjectData> othersData;
        public List<PropMetaData> metaData;
        public List<PropMetaData> userMetaData;

        public SObjectData.types Type { get { return this.currentType; } }
        public void SetType(SObjectData.types type) { this.currentType = type; }
        public List<SObjectData> GetDataByType(SObjectData.types type)
        {
            return data.FindAll(x => x.type == type);
        }

        public List<PropMetaData> GetMetadataByType(SObjectData.types type)
        {
            return metaData.FindAll(x => x.type == type);
        }

        public List<PropMetaData> GetUserMetadataByType(SObjectData.types type)
        {
            return userMetaData.FindAll(x => x.type == type);
        }

        [SerializeField] string currentID;
        [SerializeField] SObjectData.types currentType;

        SObjectData currentSO;
        Dictionary<string, ServerPartMetaData> serverPartsMetaData;


        private void Start()
        {
            FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
        }

        private void OnDestroy()
        {
            FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
        }

        void OnTokenUpdated()
        {
            Debug.Log("#OnTokenUpdated");
            if (Data.Instance.userData.IsLogged())
            {
                CancelInvoke();
                LoadSOMetadataFromServer();
            }
            else
                Invoke("OnTokenUpdated", 1);
        }
        void LoadSOMetadataFromServer()
        {
            Debug.Log("#Load SO MetadataFromServer");
            metaData = new List<PropMetaData>();
            FirebaseStoryMakerDBManager.Instance.LoadMetadataFromServer(MetadataTypes.so.ToString(), OnLoadSODataFromServer);
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

                    if (iInScene.data.soID != "")
                        sd.soID = iInScene.data.soID;
                    else
                    {
                        sd.id = iInScene.data.id;
                        sd.galleryID = iInScene.data.galleryID;

                        ItemInScene mirror = iInScene.GetMirror();
                        if (mirror != null)
                            mirrors.Add(mirror);
                    }

                    sd.part = newPartID;
                    sd.position = iInScene.data.position;
                    sd.rotation = iInScene.data.rotation;
                    sd.scale = iInScene.data.scale;
                    sd.anim = iInScene.data.anim;
                    sd.color = iInScene.data.colorName;
                    wd.items.Add(sd);

                }
                // UIManager.Instance.boardUI.items.Delete(iInScene);
            }
            currentSO = wd;

            if (wd.id == "")
            {
                data.Add(wd);
                FirebaseStoryMakerDBManager.Instance.SaveToServer(MetadataTypes.so.ToString(), wd.GetServerData(), OnSavedToServer);
            }
            else
            {
                FirebaseStoryMakerDBManager.Instance.UpdateDataToServer(MetadataTypes.so.ToString(), wd.id, wd.GetServerData(), OnSavedToServer);
            }
        }
        void OnSavedToServer(bool succes, string id)
        {
            if (currentSO.id == "") // is new
            {
                if (Data.Instance.userData.isAdmin)
                    metaData.Add(new PropMetaData() { id = id, userID = Data.Instance.userData.userDataInDatabase.uid, thumb = currentSO.thumb, type = currentSO.type });
                userMetaData.Add(new PropMetaData() { id = id, userID = Data.Instance.userData.userDataInDatabase.uid, thumb = currentSO.thumb, type = currentSO.type });

            }
            else
            {
                if (Data.Instance.userData.isAdmin)
                    metaData.Find(x => x.id == currentSO.id).thumb = currentSO.thumb;
                userMetaData.Find(x => x.id == currentSO.id).thumb = currentSO.thumb;
            }
            currentSO.id = id;
            currentID = id;

            ServerPropMetaData swmd = new ServerPropMetaData();
            swmd.thumb = System.Convert.ToBase64String(currentSO.thumb.EncodeToPNG());
            swmd.userID = Data.Instance.userData.userDataInDatabase.uid;
            swmd.AddCreator(Data.Instance.userData.userDataInDatabase.uid);
            swmd.type = currentSO.type;

            FirebaseStoryMakerDBManager.Instance.SaveMetadataToServer(MetadataTypes.so.ToString(), currentID, swmd);
            UIManager.Instance.ShowWorkDetail(currentSO);

        }
        public void OnLoadSODataFromServer(List<CharacterMetaData> sfds)
        {
            //Debug.Log("OnLoadSODataFromServer !!");
            foreach (CharacterMetaData sfd in sfds)
                metaData.Add(sfd as PropMetaData);
            userMetaData = metaData.FindAll(x => x.userID == Data.Instance.userData.userDataInDatabase.uid);
            data = new();
            FirebaseStoryMakerDBManager.Instance.LoadUserAssetsFromServer(MetadataTypes.so.ToString(), LoadAssetsFromServer);
        }

        void LoadAssetsFromServer(Dictionary<string, SObjectServerData> d)
        {
            foreach (KeyValuePair<string, SObjectServerData> e in d)
            {
                //Debug.Log("#Load SO FromServer " + e.Key + ": " + e.Value);
                SObjectData wd = new SObjectData();
                wd.id = e.Key;
                wd.LoadServerData(e.Value);
                PropMetaData p = metaData.Find(x => x.id == wd.id);
                wd.thumb = p?.thumb;
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
            SObjectData o = data.Find(x => x.id == id);
            if (o != null)
                currentType = o.type;
            return o;
        }

        public void LoadOthersObject(string id, System.Action<SObjectData> OnDone)
        {
            currentID = "";
            //  Debug.Log(currentID);
            if (othersData == null)
                othersData = new List<SObjectData>();
            SObjectData chd = othersData.Find(x => x.id == id);
            if (chd != null)
            {
                currentType = chd.type;
                OnDone(chd);
            }
            else
            {
                PropMetaData chmd = metaData.Find(x => x.id == id);
                if (chmd == null)
                {
                    Debug.Log("Fail getting Prop Meta Data");
                    OnDone(null);
                    return;
                }

                FirebaseStoryMakerDBManager.Instance.LoadAssetFromServer(id, (success, key, data) =>
                {
                    if (success)
                    {
                        SObjectData chD = othersData.Find(x => x.id == key);
                        if (chD != null)
                        {
                            Debug.Log("& othersData != null");
                            currentType = chD.type;
                            OnDone(chD);
                            return;
                        }
                        chD = new SObjectData();
                        chD.id = key;
                        chD.LoadServerData(data);
                        chD.thumb = chmd.thumb;
                        chD.type = chmd.type;
                        othersData.Add(chD);
                        OnDone(chD);
                    }
                    else
                    {
                        Debug.Log("Fail getting Character Data");
                        OnDone(null);
                    }

                }, chmd.userID);

                Debug.Log("& LoadOthersObject " + id);

            }

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
