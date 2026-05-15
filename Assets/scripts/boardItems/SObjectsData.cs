using BoardItems.BoardData;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.StoryMaker.DB;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace BoardItems
{
    public class SObjectsData : MonoBehaviour
    {
        [field: SerializeField] public Vector2Int ThumbSize { get; private set; }

        public List<SObjectData> data;
        public List<SObjectData> othersData;
        public List<PropMetaData> metaData;
        public List<PropMetaData> userMetaData;

        string initTimeStamp;

        [SerializeField] SObjectData.types currentType;
        public SObjectData.types Type { 
            get { return this.currentType; }
            set
            {
                print("Type" + currentType);
                this.currentType = value;
            }
        }
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
            wd.thumb = TextureUtils.GPUScaleTexture(tex, ThumbSize.x, ThumbSize.y);            
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
            string tstamp = DateTime.UtcNow.ToString("o");
            if (currentSO.id == "") // is new
            {                
                if (Data.Instance.userData.isAdmin)  
                    metaData.Add(new PropMetaData() { id = id, userID = Data.Instance.userData.userDataInDatabase.uid, thumb = currentSO.thumb, type = currentSO.type, timestamp=tstamp });
                userMetaData.Add(new PropMetaData() { id = id, userID = Data.Instance.userData.userDataInDatabase.uid, thumb = currentSO.thumb, type = currentSO.type, timestamp = tstamp });

            }
            else
            {
                    if (Data.Instance.userData.isAdmin) {
                        PropMetaData md = metaData.Find(x => x.id == currentSO.id);
                        md.thumb = currentSO.thumb;
                        md.timestamp = tstamp;
                    }
                    PropMetaData umd = userMetaData.Find(x => x.id == currentSO.id);
                    umd.thumb = currentSO.thumb;
                    umd.timestamp = tstamp;
            }

            metaData = metaData.OrderByDescending(x => x.timestamp).ToList();
            userMetaData = userMetaData.OrderByDescending(x => x.timestamp).ToList();

            currentSO.id = id;
            currentID = id;

            ServerPropMetaData swmd = new ServerPropMetaData();
            swmd.userID = Data.Instance.userData.userDataInDatabase.uid;
            swmd.AddCreator(Data.Instance.userData.userDataInDatabase.uid);
            swmd.type = currentSO.type;
            swmd.timestamp = tstamp;


            FirebaseStoryMakerDBManager.Instance.SaveMetadataToServer(MetadataTypes.so.ToString(), currentID, swmd);

            FirebaseStoryMakerDBManager.Instance.UploadTexture(currentSO.thumb, MetadataTypes.so.ToString(), currentSO.id, Data.Instance.userData.userDataInDatabase.uid);

            UIManager.Instance.ShowWorkDetail(currentSO);

        }
        public void OnLoadSODataFromServer(List<CharacterMetaData> sfds)
        {
            //Debug.Log("% OnLoadSODataFromServer !!");
            foreach (CharacterMetaData sfd in sfds.OrderByDescending(x => x.timestamp).ToList()) {
                PropMetaData pmd = sfd as PropMetaData;
                metaData.Add(pmd);
                FirebaseStoryMakerDBManager.Instance.DownloadTexture(MetadataTypes.so.ToString(), pmd.id, (tex) => {
                    pmd.thumb = tex;
                    if (pmd.userID == Data.Instance.userData.userDataInDatabase.uid) {
                        SObjectData od = data.Find(x => x.id == pmd.id);
                        if (od != null)
                            od.thumb = pmd.thumb;
                    }
                }, pmd.userID);

                /* FirebaseStoryMakerDBManager.Instance.UploadTexture(sfd.thumb, MetadataTypes.so.ToString(), sfd.id, sfd.userID, () => {
                     DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("metadata/so/" + sfd.id + "/thumb");
                     reference.RemoveValueAsync().ContinueWithOnMainThread(task => {
                         if (task.IsFaulted || task.IsCanceled) {
                             Debug.Log("#Delete SO thumb FAIL");
                             Debug.Log(task.Exception);
                         } else {
                             Debug.Log("#Delete SO thumb metadata/characters/" + sfd.id);
                         }
                     });
                 });*/

            }
            userMetaData = metaData.FindAll(x => x.userID == Data.Instance.userData.userDataInDatabase.uid);
            data = new();
            FirebaseStoryMakerDBManager.Instance.LoadUserAssetsFromServer(MetadataTypes.so.ToString(), LoadAssetsFromServer);

            initTimeStamp = DateTime.UtcNow.ToString("o");
            var partMetadata = FirebaseDatabase.DefaultInstance.GetReference("metadata/so/");
            partMetadata.ChildAdded += OnPropAdded;
            partMetadata.ChildChanged += OnPropChanged;
            partMetadata.ChildRemoved += OnPropRemoved;
        }

        void OnPropAdded(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            DataSnapshot snapshot = args.Snapshot;
            if (snapshot.Exists) {
                if (!snapshot.HasChild("timestamp"))
                    return;

                string timestamp = snapshot.Child("timestamp").Value as string;
                if (timestamp == null || timestamp == "" || String.Compare(timestamp, initTimeStamp) < 0)
                    return;

                Debug.Log("% OnPartAdded: " + timestamp + " > " + initTimeStamp);
                PropMetaData pmd = metaData.Find(x => x.id == snapshot.Key);
                if (pmd != null) {
                    SetPropChanged(pmd, snapshot);
                } else {

                    pmd = new PropMetaData();
                    pmd.type = (SObjectData.types)((int)(long)snapshot.Child("type").Value);
                    pmd.id = snapshot.Key;
                    pmd.userID = snapshot.Child("userID").Value as string;
                    pmd.creators = new List<string>();
                    if (snapshot.HasChild("timestamp"))
                        pmd.timestamp = snapshot.Child("timestamp").Value as string;
                    else
                        pmd.timestamp = DateTime.MinValue.ToUniversalTime().ToString("o");

                    if (snapshot.HasChild("creators")) {
                        foreach (var uid in snapshot.Child("creators").Children)
                            pmd.creators.Add(uid.Value as string);
                    }
                    
                    FirebaseStoryMakerDBManager.Instance.DownloadTexture(MetadataTypes.so.ToString(), pmd.id, (tex) => {
                        pmd.thumb = tex;
                        Events.OnPropMetadataAdded(pmd);
                    }, pmd.userID);
                    metaData.Insert(0, pmd);

                    if (pmd.userID == Data.Instance.userData.userDataInDatabase.uid)
                        userMetaData.Insert(0, pmd);                    
                }
            }
        }

        void OnPropChanged(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            Debug.Log("% OnPartChanged: " + args.Snapshot.GetRawJsonValue());

            DataSnapshot snapshot = args.Snapshot;
            if (snapshot.Exists) {
                PropMetaData pmd = metaData.Find(x => x.id == snapshot.Key);
                if (pmd == null) {
                    return;
                }

                SetPropChanged(pmd, snapshot);
                /*if (sfd.userID == Data.Instance.userData.userDataInDatabase.uid)
                    userFilmsData.Insert(0, fd);*/
            }
        }

        void SetPropChanged(PropMetaData pmd, DataSnapshot child) {
            if (child.HasChild("timestamp"))
                pmd.timestamp = child.Child("timestamp").Value as string;
            else
                pmd.timestamp = DateTime.MinValue.ToUniversalTime().ToString("o");

            FirebaseStoryMakerDBManager.Instance.DownloadTexture(MetadataTypes.so.ToString(), pmd.id, (tex) => {
                pmd.thumb = tex;
                Events.OnPropMetadataUpdated(pmd);
            }, pmd.userID);

            metaData = metaData.OrderByDescending(x => x.timestamp).ToList();
            userMetaData = userMetaData.OrderByDescending(x => x.timestamp).ToList();            

        }

        void OnPropRemoved(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            Debug.Log("% OnPartRemoved: " + args.Snapshot.GetRawJsonValue());

            DataSnapshot snapshot = args.Snapshot;
            if (snapshot.Exists) {
                PropMetaData pmd = metaData.Find(x => x.id == snapshot.Key);
                if (pmd != null) {
                    metaData.Remove(pmd);
                    userMetaData.Remove(pmd);
                    Events.OnPropMetadataRemoved(pmd);
                }
            }
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
                Type = o.type;
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
                Type = chd.type;
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
                            Type = chD.type;
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


        System.Action<bool, string> OnDuplicated;
        PropMetaData pdata;
        public void Duplicate(string id, System.Action<bool, string> OnDuplicated)
        {
            this.OnDuplicated = OnDuplicated;
            print("duplicate SO " + id);
            string otherUserID;
            pdata = metaData.Find(x => x.id == id);
            otherUserID = pdata.userID;
            if (data == null)
            {
                Debug.LogError("No se ha encontrado el character a duplicar");
                return;
            }
            FirebaseStoryMakerDBManager.Instance.LoadAssetFromServer(id, (success, key, d) =>
            {
                if (success)
                {
                    currentSO = new SObjectData();
                    currentSO.id = key;
                    currentSO.LoadServerData(d);
                    currentSO.thumb = pdata.thumb;
                    currentSO.type = pdata.type;
                    data.Add(currentSO);
                    FirebaseStoryMakerDBManager.Instance.SaveToServer(MetadataTypes.so.ToString(), currentSO.GetServerData(), OnDuplicatedSaved);
                }
                else
                {
                    Debug.Log("Fail getting Character Data");
                }

            }, otherUserID);




        }
        void OnDuplicatedSaved(bool success, string id)
        {
            if (success)
            {
                string tstamp = DateTime.UtcNow.ToString("o");

                metaData = metaData.OrderByDescending(x => x.timestamp).ToList();

                currentSO.id = id;
                currentID = id;

                ServerPropMetaData swmd = new ServerPropMetaData();

                swmd.AddCreator(Data.Instance.userData.userDataInDatabase.uid);
                swmd.userID = Data.Instance.userData.userDataInDatabase.uid;
                swmd.timestamp = tstamp;
                FirebaseStoryMakerDBManager.Instance.SaveMetadataToServer(MetadataTypes.so.ToString(), currentID, swmd);

                FirebaseStoryMakerDBManager.Instance.UploadTexture(pdata.thumb, MetadataTypes.characters.ToString(), id, Data.Instance.userData.userDataInDatabase.uid);

                // OpenCharacterDetail(currentCharacter);
            }
            OnDuplicated?.Invoke(success, id);
        }



    }

}
