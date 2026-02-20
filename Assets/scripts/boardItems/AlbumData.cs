using BoardItems.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UI;
using UnityEngine;
using UnityEngine.Windows;
using Yaguar.Auth;
using Yaguar.StoryMaker.DB;

namespace BoardItems
{
    public class AlbumData : MonoBehaviour {
        public Vector2Int thumbSize;
        // public List<CharacterData> pakapakaAlbum;
        public List<CharacterData> characters;

        public List<CharacterData> heads;
        public List<CharacterData> bellies;
        public List<CharacterData> hands;
        public List<CharacterData> feet;
        public List<CharacterData> hairs;
        public List<CharacterData> faces;

        public List<CharacterMetaData> charactersMetaData;

        string fieldSeparator = ":";
        string itemSeparator = "&";
        string itemFieldSeparator = "#";

        [SerializeField] string currentID;

        CharacterData currentCharacter;
        Dictionary<string, ServerPartMetaData> serverPartsMetaData;

        [SerializeField] string loadedPresetId;

        [Serializable]
        public class CharacterPartServerData
        {
            public List<ServerIData> items;            
        }

        [Serializable]
        public class CharacterServerData : CharacterPartServerData
        {
            public PalettesManager.colorNames armsColor;
            public PalettesManager.colorNames legsColor;
            public PalettesManager.colorNames eyebrowsColor;
        }

        [Serializable]
        public class CharacterPartData
        {
            public string id;
            public Texture2D thumb;
            public List<SavedIData> items;

            public Sprite GetSprite() {
                if (thumb == null) {
                    Debug.LogError("No hay thumb para character id: " + id);
                    return null;
                }
                return Sprite.Create(thumb, new Rect(0, 0, thumb.width, thumb.height), Vector2.zero);
            }            
        }

        [Serializable]
        public class CharacterData : CharacterPartData
        {
            public PalettesManager.colorNames armsColor;
            public PalettesManager.colorNames legsColor;
            public PalettesManager.colorNames eyebrowsColor;            

            public CharacterServerData GetServerData() {
                CharacterServerData csd = new CharacterServerData();
                List<ServerIData> csdItems = new List<ServerIData>();
                foreach(SavedIData sid in items) {
                    csdItems.Add(sid.GetServerIData());
                }
                csd.items = csdItems;
                csd.armsColor = armsColor;
                csd.legsColor = legsColor;
                csd.eyebrowsColor = eyebrowsColor;
                return csd;
            }

            public void LoadServerData(CharacterServerData serverData) {
                items = new List<SavedIData>();
                Debug.Log(serverData.items == null);
                Debug.Log(serverData.items.Count);
                foreach (ServerIData sid in serverData.items) {
                    SavedIData savedData = new SavedIData();
                    savedData.LoadServerData(sid);
                    items.Add(savedData);
                }
                armsColor = serverData.armsColor;
                legsColor = serverData.legsColor;
                eyebrowsColor = serverData.eyebrowsColor;
            }
        }

        [Serializable]
        public class SavedIData
        {
            public int galleryID;
            public int part;
            public int id;
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;
            public AnimationsManager.anim anim;
            public PalettesManager.colorNames color;

            public ServerIData GetServerIData() {
                ServerIData sid = new ServerIData();
                sid.galleryID = galleryID;
                sid.id = id;
                sid.part = part;
                sid.position = new V3(position);
                sid.rotation = new V3(rotation);
                sid.scale = new V3(scale);
                sid.anim = anim;
                sid.color = color;
                return sid;
            }

            public void LoadServerData(ServerIData serverData) {
                id = serverData.id;
                galleryID = serverData.galleryID;
                part = serverData.part;
                position = new Vector3(serverData.position.x, serverData.position.y, serverData.position.z);
                rotation = new Vector3(serverData.rotation.x, serverData.rotation.y, serverData.rotation.z);
                scale = new Vector3(serverData.scale.x, serverData.scale.y, serverData.scale.z);
                anim = serverData.anim;
                color = serverData.color;
                Debug.Log(galleryID + ", " + part + ", " + position + ", " + rotation + ", " + scale + ", " + anim + ", " + color);
            }
        }

        [Serializable]
        public class ServerIData
        {
            public int galleryID;
            public int part;
            public int id;

            public V3 position;
            public V3 rotation;
            public V3 scale;
            public AnimationsManager.anim anim;
            public PalettesManager.colorNames color;
        }

        public struct V3
        {
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }

            public V3(float x, float y, float z) {
                this.x = x;
                this.y = y;
                this.z = z;
            }
            public V3(Vector3 v) {
                this.x = v.x;
                this.y = v.y;
                this.z = v.z;
            }
        }        

        [Serializable]
        public class CharacterMetaData {
            public string id;
            public Texture2D thumb;
            public string userID;
        }

        [Serializable]
        public class ServerCharacterMetaData {
            public string thumb;
            public string userID;
        }

        [Serializable]
        public class ServerPartMetaData
        {
            public string thumb;
            public string partID;
        }

        /*public enum PartIds {
            none = 0,
            head = 1,
            belly = 2,
            hands = 3,
            feet = 4,
            hair = 7,
            face = 8
        }*/

        private void Start() {
            FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
            Events.OnPresetReset += OnPresetReset;
            Events.OnPresetLoaded += OnPresetLoaded;
            Events.OnNewCharacter += OnNewCharacter;
            /*if(charactersMetaData.Count<1)
                LoadUserCharacterMetadataFromServer();*/

            // PlayerPrefs.DeleteAll();
            //StartCoroutine(LoadWorks());
        }

       

        private void OnDestroy() {
            FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
            Events.OnPresetReset -= OnPresetReset;
            Events.OnPresetLoaded -= OnPresetLoaded;
            Events.OnNewCharacter -= OnNewCharacter;
        }

        void OnTokenUpdated() {
            Debug.Log("#OnTokenUpdated");
            if (Data.Instance.userData.IsLogged()) {
                CancelInvoke();
                LoadUserCharacterMetadataFromServer();                
            } else
                Invoke("OnTokenUpdated", 1);
        }
        void LoadUserCharacterMetadataFromServer() {
            Debug.Log("#LoadUserCharacterMetadataFromServer");
            //if (Data.Instance.userData.IsLogged()) {
                Debug.Log("#is Logged");
                charactersMetaData = new List<CharacterMetaData>();
                FirebaseStoryMakerDBManager.Instance.LoadUserCharacterMetadataFromServer(OnUserLoadCharacterDataFromServer);
            //}
        }

        /*void LoadPartMetadataFromServer() {
            Debug.Log("#LoadPartMetadataFromServer");
            //if (Data.Instance.userData.IsLogged()) {
                FirebaseStoryMakerDBManager.Instance.LoadBodypartPresetMetadataFromServer(OnLoadPresetMetadataFromServer);
            //}
        }

        public void OnLoadPresetMetadataFromServer(Dictionary<string, ServerPartMetaData> spmd) {
            Debug.Log("#OnLoadPresetMetadataFromServer");
            serverPartsMetaData = spmd;
            LoadPresetsFromServer();
        }

        int loadedParts = 0;
        void LoadPresetsFromServer() {
            if (loadedParts + 1 >= BoardItems.Characters.CharacterData.GetServerPartsLength()) 
                return;
            loadedParts++;
            string partName = BoardItems.Characters.CharacterData.GetServerUniquePartsId(loadedParts);
            Debug.Log("#partName "+partName + ":  " + loadedParts);
            if (partName == null) {
                LoadPresetsFromServer();
                return;
            }
            FirebaseStoryMakerDBManager.Instance.LoadBodypartPresetsFromServer(loadedParts, OnLoadPresetsFromServer);
        }
        
        void OnLoadPresetsFromServer(int partId, Dictionary<string,string> data) {            
            if (data != null) {
                GetPreset(partId).Clear();
                LoadCharactersFromServer(data);
            }
            LoadPresetsFromServer();
        }*/

        public void SaveCharacter(Texture2D tex)
        {
            CharacterData wd;
            if (currentID == "" || currentID == null)
            {
                wd = new CharacterData();
                wd.id = "";
            }
            else
                wd = GetCharacter(currentID);
            CharacterManager cm = UIManager.Instance.boardUI.characterManager;

            wd.armsColor = cm.GetArmsColor();
            wd.legsColor = cm.GetLegsColor();
            wd.eyebrowsColor = cm.GetEyebrowsColor();

            wd.thumb = tex;
            wd.items = new List<SavedIData>();
            int totalItems = UIManager.Instance.boardUI.items.all.Count;
            int totalParts = 0;
            int partID = 0;
            int i = 0;
            while (i < totalItems)
            {
                ItemInScene iInScene = UIManager.Instance.boardUI.items.all[0];
                int newPartID = (int)iInScene.data.part;
                if (partID != newPartID)
                    totalParts++;
                partID = newPartID;
                if (partID > 0)
                {
                    SavedIData sd = new SavedIData();
                    sd.part = partID;
                    sd.id = iInScene.data.id;
                    sd.position = iInScene.data.position;
                    sd.rotation = iInScene.data.rotation;
                    sd.scale = iInScene.data.scale;
                    sd.anim = iInScene.data.anim;
                    sd.color = iInScene.data.colorName;
                    sd.galleryID = iInScene.data.galleryID;
                    wd.items.Add(sd);
                    bool mirrorDeleted = UIManager.Instance.boardUI.items.Delete(iInScene);
                    if (mirrorDeleted)
                        i++;
                }
                i++;
            }
            print("SAVE data: totalparts" + totalParts + " lastPArtID: "+ partID);
            currentCharacter = wd;

            if (totalParts > 1) { // is a complete character;
                if (wd.id == "") {
                    characters.Add(wd);
                    //FirebaseStoryMakerDBManager.Instance.SaveCharacterToServer(EncodeCharacterData(wd), OnCharacterSavedToServer);
                    FirebaseStoryMakerDBManager.Instance.SaveCharacterToServer(wd.GetServerData(), OnCharacterSavedToServer);
                } else {
                    FirebaseStoryMakerDBManager.Instance.UpdateCharacterToServer(wd.id, wd.GetServerData(), OnCharacterSavedToServer);
                }
            } else if (Data.Instance.userData.isAdmin) { // is a part preset;
                if (loadedPresetId == "") {
                    AddPart(partID, wd);
                    FirebaseStoryMakerDBManager.Instance.SaveBodypartPresetToServer(EncodeCharacterData(wd), BoardItems.Characters.CharacterData.GetServerPartsId(partID), OnPresetSavedToServer);                    
                } else {
                    FirebaseStoryMakerDBManager.Instance.UpdateBodypartPresetToServer(loadedPresetId, EncodeCharacterData(wd), BoardItems.Characters.CharacterData.GetServerPartsId(partID), OnPresetSavedToServer);
                }
            }            

            //PersistThumbLocal(wd);
            // SetPkpkShared(wd, false);
        }
        string EncodeCharacterData(CharacterData wd)
        {
            string workData = "";
            workData += Enum.GetName(typeof(PalettesManager.colorNames), wd.armsColor) + fieldSeparator;
            workData += Enum.GetName(typeof(PalettesManager.colorNames), wd.legsColor) + fieldSeparator;
            workData += Enum.GetName(typeof(PalettesManager.colorNames), wd.eyebrowsColor) + fieldSeparator;
            for (int i = 0; i < wd.items.Count; i++)
            {
                workData += wd.items[i].galleryID + itemFieldSeparator + wd.items[i].id + itemFieldSeparator + SetRealFloat(wd.items[i].position.x) + itemFieldSeparator +
                SetRealFloat(wd.items[i].position.y) + itemFieldSeparator + SetRealFloat(wd.items[i].position.z) + itemFieldSeparator + SetRealFloat(wd.items[i].rotation.z) +
                itemFieldSeparator + SetRealFloat(wd.items[i].scale.x) +
                itemFieldSeparator + Enum.GetName(typeof(PalettesManager.colorNames), wd.items[i].color) +
                itemFieldSeparator + Enum.GetName(typeof(AnimationsManager.anim), wd.items[i].anim) +
                itemFieldSeparator + wd.items[i].part;
                if (i < wd.items.Count - 1)
                    workData += itemSeparator;
            }
            Debug.Log("#workData: " + workData);
            return workData;
        }
        string SetRealFloat(float f)
        {
            string result = f.ToString("F4", CultureInfo.InvariantCulture);
            return result;
        }
        void OnCharacterSavedToServer(bool succes, string id)
        {
            currentCharacter.id = id;
            currentID = id;

            ServerCharacterMetaData swmd = new ServerCharacterMetaData();
            swmd.thumb = System.Convert.ToBase64String(currentCharacter.thumb.EncodeToPNG());
            swmd.userID = Data.Instance.userData.userDataInDatabase.uid;
            FirebaseStoryMakerDBManager.Instance.SaveCharacterMetadataToServer(currentID, swmd);

            OpenCharacterDetail(currentCharacter);
        }

        void OnPresetSavedToServer(bool succes, string id, string partId) {            
            loadedPresetId = id;
            if (loadedPresetId == "") {
                Debug.Log("#Error en update preset metadata");
                return;
            }
            ServerPartMetaData swmd = new ServerPartMetaData();
            swmd.thumb = System.Convert.ToBase64String(currentCharacter.thumb.EncodeToPNG());
            swmd.partID = partId;
            FirebaseStoryMakerDBManager.Instance.SaveBodypartPresetMetadataToServer(id, swmd);

            OpenCharacterDetail(currentCharacter);
        }

        void OpenCharacterDetail(CharacterData wd)
        {
            UIManager.Instance.ShowWorkDetail(wd);
            //Sprite sprite = Sprite.Create(wd.thumb, new Rect(0, 0, wd.thumb.width, wd.thumb.height), Vector2.zero);
            //UIManager.Instance.workDetailUI.ShowWorkDetail(wd.id, sprite, true);
            //Events.ResetItems();
        }

      //  void PersistThumbLocal(CharacterData wd)
     //   {
            //if (wd.id == "")
            //    wd.id = System.DateTime.Now.ToString("yyyyMMddHHmmss");

            //byte[] bytes = wd.thumb.EncodeToPNG();
            //string folder = Path.Combine(Application.persistentDataPath, "Thumbs");
            //if (!Directory.Exists(folder))
            //    Directory.CreateDirectory(folder);
            //string filename = Path.Combine(folder, "thumb_" + wd.id + ".png");

            //System.IO.File.WriteAllBytes(filename, bytes);
            //Debug.Log(string.Format("thumb to: {0}", filename));

            //OpenCharacterDetail(wd);

           // PersistWorkDataLocal(wd.id, wd);
     //   }

        //void PersistWorkDataLocal(string id, CharacterData wd)
        //{
        //    string workData = "";
        //    workData += Enum.GetName(typeof(PalettesManager.colorNames), wd.bgColorName) + fieldSeparator;
        //    for (int i = 0; i < wd.items.Count; i++)
        //    {
        //        workData += wd.items[i].galleryID + itemFieldSeparator + wd.items[i].id + itemFieldSeparator + wd.items[i].position.x + itemFieldSeparator +
        //        wd.items[i].position.y + itemFieldSeparator + wd.items[i].position.z + itemFieldSeparator + wd.items[i].rotation.z +
        //        itemFieldSeparator + wd.items[i].scale.x +
        //        itemFieldSeparator + Enum.GetName(typeof(PalettesManager.colorNames), wd.items[i].color) +
        //        itemFieldSeparator + Enum.GetName(typeof(AnimationsManager.anim), wd.items[i].anim) +
        //        itemFieldSeparator + wd.items[i].part;
        //        if (i < wd.items.Count - 1)
        //            workData += itemSeparator;
        //    }

        //    PlayerPrefs.SetString("Work_" + id, workData);
        //    PersistWorksIds();
        //}

        //void PersistWorksIds()
        //{
        //    string workIDs = "";
        //    foreach (CharacterData wd in characters)
        //        workIDs += wd.id + fieldSeparator;
        //    foreach (CharacterData wd in heads)
        //        workIDs += wd.id + fieldSeparator;
        //    foreach (CharacterData wd in bellies)
        //        workIDs += wd.id + fieldSeparator;
        //    foreach (CharacterData wd in hands)
        //        workIDs += wd.id + fieldSeparator;
        //    foreach (CharacterData wd in feet)
        //        workIDs += wd.id + fieldSeparator;
        //    foreach (CharacterData wd in hairs)
        //        workIDs += wd.id + fieldSeparator;
        //    foreach (CharacterData wd in faces)
        //        workIDs += wd.id + fieldSeparator;

        //    PlayerPrefs.SetString("WorksIds", workIDs);
        //}        

        public void OnUserLoadCharacterDataFromServer(List<CharacterMetaData> sfds)
        {
            /*foreach (KeyValuePair<string, ServerCharacterMetaData> e in sfds)
            {
                if (charactersMetaData.Find(x => x.id == e.Key) == null)
                {
                    CharacterMetaData fd = new CharacterMetaData();
                    fd.id = e.Key;
                    fd.userID = e.Value.userID;
                    fd.thumb = new Texture2D(1, 1);
                    fd.thumb.LoadImage(System.Convert.FromBase64String(e.Value.thumb));
                    charactersMetaData.Add(fd);
                }
            }*/
            charactersMetaData = sfds;
            characters = new();
            FirebaseStoryMakerDBManager.Instance.LoadUserCharactersFromServer(LoadCharactersFromServer);
        }

        void LoadCharactersFromServer(Dictionary<string, CharacterServerData> data)
        {
            foreach (KeyValuePair<string, CharacterServerData> e in data)
            {
                Debug.Log("#LoadCharactersFromServer " + e.Key + ": " + e.Value);
                CharacterData wd = new CharacterData();
                wd.id = e.Key;
                Debug.Log("#Aca1: "+ e.Value.items.Count);
                wd.LoadServerData(e.Value);
                Debug.Log("#Aca2");
                wd.thumb = charactersMetaData.Find(x => x.id == wd.id)?.thumb;
                characters.Add(wd);
                /*string[] wData = e.Value.Split(fieldSeparator[0]);
                print("total art: " + wData.Length);
                if (wData[0] != "") {
                    Debug.Log("bgColorIndex: " + wData[0]);
                    wd.armsColor = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), wData[0]);
                    if(wData.Length>2)
                        wd.legsColor = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), wData[1]);
                    if (wData.Length > 3)
                        wd.eyebrowsColor = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), wData[2]);

                    List<SavedIData> items = new List<SavedIData>();
                    string[] itemsData = wData[wData.Length-1].Split(itemSeparator[0]);
                    Debug.Log("ItemCount: " + itemsData.Length);

                    int totalParts = 0;
                    int partID = 0;

                    for (int j = 0; j < itemsData.Length; j++) {
                        string[] iData = itemsData[j].Split(itemFieldSeparator[0]);

                        //int num = 0;
                        //foreach (string s in iData) {
                        //    Debug.Log(num + "___ " + s);
                        //    num++;
                        //}
                        //ItemData iD = Data.Instance.galeriasData.GetItem(wd.galleryID,int.Parse(iData[0]));
                        //if (iD.sprite == null)
                        //    Debug.Log(iD.id + ": spriteNull");
                        SavedIData sd = new SavedIData();
                        Debug.Log("# " + iData[0]);
                        sd.galleryID = int.Parse(iData[0], System.Globalization.CultureInfo.InvariantCulture);
                        sd.id = int.Parse(iData[1], System.Globalization.CultureInfo.InvariantCulture);
                        Debug.Log("# " + iData[1]);
                        sd.position = new Vector3(SetFloat(iData[2]), SetFloat(iData[3]), SetFloat(iData[4]));
                        sd.rotation = new Vector3(0f, 0f, SetFloat(iData[5]));
                        sd.scale = new Vector3(SetFloat(iData[6]), SetFloat(iData[6]), 0f);
                        sd.color = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), iData[7]);
                        sd.anim = (AnimationsManager.anim)Enum.Parse(typeof(AnimationsManager.anim), iData[8]);
                        int newPartID = int.Parse(iData[9], System.Globalization.CultureInfo.InvariantCulture);
                        if (newPartID != partID)
                            totalParts++;
                        partID = newPartID;
                        sd.part = partID;
                        // iD.color = Color.white;
                        items.Add(sd);
                    }
                    wd.items = items;

                    if (totalParts > 1) {  //is full character:
                        wd.thumb = charactersMetaData.Find(x => x.id == wd.id)?.thumb;
                        characters.Add(wd);
                    } else {  //is preset part:
                        wd.thumb = new Texture2D(1, 1);
                        wd.thumb.LoadImage(System.Convert.FromBase64String(serverPartsMetaData[wd.id].thumb));
                        AddPart(partID, wd);
                    }
                }*/
            }

            //LoadPartMetadataFromServer();
        }
        float SetFloat(string f)
        {
            return float.Parse(f, CultureInfo.InvariantCulture);
        }
        //IEnumerator LoadWorks()
        //{
        //    string[] workIDs = PlayerPrefs.GetString("WorksIds").Split(fieldSeparator[0]);
        //    for (int i = 0; i < workIDs.Length - 1; i++)
        //    {
        //        CharacterData wd = new CharacterData();
        //        wd.id = workIDs[i];
        //        string[] wData = PlayerPrefs.GetString("Work_" + workIDs[i]).Split(fieldSeparator[0]);
        //        print("total art: " + wData.Length);

        //        if (wData[0] != "")
        //        {
        //            wd.bgColorName = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), wData[0]);

        //            List<CharacterData.SavedIData> items = new List<CharacterData.SavedIData>();
        //            string[] itemsData = wData[1].Split(itemSeparator[0]);

        //            int totalParts = 0;
        //            int partID = 0;
        //            for (int j = 0; j < itemsData.Length; j++)
        //            {
        //                string[] iData = itemsData[j].Split(itemFieldSeparator[0]);

        //                int num = 0;
        //                foreach (string s in iData)
        //                {
        //                    Debug.Log(num + "___ " + s);
        //                    num++;
        //                }
        //                CharacterData.SavedIData sd = new CharacterData.SavedIData();
        //                sd.galleryID = int.Parse(iData[0]);
        //                sd.id = int.Parse(iData[1]);
        //                sd.position = new Vector3(SetFloat(iData[2]), SetFloat(iData[3]), SetFloat(iData[4]));
        //                sd.rotation = new Vector3(0f, 0f, SetFloat(iData[5]));
        //                sd.scale = new Vector3(SetFloat(iData[6]), SetFloat(iData[6]), 0f);
        //                sd.color = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), iData[7]);
        //                sd.anim = (AnimationsManager.anim)Enum.Parse(typeof(AnimationsManager.anim), iData[8]);
        //                int newPartID = int.Parse(iData[9]);
        //                if (newPartID != partID)
        //                    totalParts++;
        //                partID = newPartID;
        //                sd.part = partID;
        //                print("new partID " + partID + " totalParts " + totalParts);
        //                items.Add(sd);
        //            }
        //            wd.items = items;

        //            string folder = Path.Combine(Application.persistentDataPath, "Thumbs");
        //            if (!Directory.Exists(folder))
        //                Directory.CreateDirectory(folder);
        //            string filename = Path.Combine(folder, "thumb_" + workIDs[i] + ".png");
        //            wd.thumb = TextureUtils.LoadLocal(filename);
        //            print("partID " + partID + " totalParts " + totalParts);
        //            if (totalParts > 1) //is full character:
        //                characters.Add(wd);
        //            else //is preset part:
        //                AddPart(partID, wd);
        //        }
        //    }
        //    yield return null;
        //}

        /*WorkData ParseWork() {

        }*/
        public string GetCurrent()
        {
            return currentID;
        }
        public CharacterData GetCharacter(string id)
        {
            return characters.Find(x => x.id == id);
        }

        public CharacterData SetCurrentID(string id)
        {
            currentID = id;
            //  Debug.Log(currentID);
            return characters.Find(x => x.id == id);
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

        //public void SetPkpkShared(string id, bool enable)
        //{
        //    WorkData wd = characters.Find(x => x.id == id);
        //    SetPkpkShared(wd, enable);
        //}

        //public void SetPkpkShared(WorkData wd, bool enable)
        //{
        //  //  wd.pkpkShared = enable;
        //  //  PlayerPrefs.SetInt("PkpkShared_" + wd.id, enable ? 1 : 0);
        //    UIManager.Instance.workDetailUI.SetSendedSign(wd.id, enable);
        //}

        //public bool IsPkpkShared(string id)
        //{
        //    return characters.Find(x => x.id == id).pkpkShared;
        //}

        public bool HasAnims(string id)
        {
            CharacterData wd = characters.Find(x => x.id == id);
            bool hasAnims = false;
            foreach (SavedIData item in wd.items)
            {
                if (item.anim != AnimationsManager.anim.NONE)
                {
                    hasAnims = true;
                    return hasAnims;
                }
            }
            return hasAnims;
        }


        void AddPart(int partID, CharacterData wd)
        {
            switch (partID)
            {
                case (int)BoardItems.Characters.CharacterData.parts.HEAD: //head
                    heads.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterData.parts.BODY: //belly
                    bellies.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterData.parts.HAND: //hand
                    hands.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterData.parts.HAND_LEFT: //hand
                    hands.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterData.parts.FOOT: //foot
                    feet.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterData.parts.FOOT_LEFT: //foot
                    feet.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterData.parts.HAIR: //hairs
                    hairs.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterData.parts.FACE: //faces
                    faces.Add(wd);
                    break;
            }
        }
        public List<CharacterData> GetPreset(int partID)
        {
            switch (partID)
            {
                case (int)BoardItems.Characters.CharacterData.parts.HEAD: //head
                    return heads;
                case (int)BoardItems.Characters.CharacterData.parts.BODY: //belly
                    return bellies;
                case (int)BoardItems.Characters.CharacterData.parts.HAND: //hand
                    return hands;
                case (int)BoardItems.Characters.CharacterData.parts.HAND_LEFT: //hand
                    return hands;
                case (int)BoardItems.Characters.CharacterData.parts.FOOT: //foot
                    return feet;
                case (int)BoardItems.Characters.CharacterData.parts.FOOT_LEFT: //foot
                    return feet;
                case (int)BoardItems.Characters.CharacterData.parts.HAIR: //hairs
                    return hairs;
                case (int)BoardItems.Characters.CharacterData.parts.FACE: //faces
                    return faces;
            }
            return null;
        }
        void OnPresetLoaded(string presetId) {
            Debug.Log("# OnPresetLoaded");
            loadedPresetId = presetId;
        }

        void OnNewCharacter() {
            Debug.Log("# OnNewCharacter");
            loadedPresetId = "";
        }
        private void OnPresetReset()
        {
            Debug.Log("# On Preset Reseted");
            loadedPresetId = "";
        }
    }

}
