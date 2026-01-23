using BoardItems.Characters;
using BoardItems.UI;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;
using static BoardItems.Characters.CharacterData;

namespace BoardItems
{
    public class AlbumData : MonoBehaviour
    {
        public Vector2Int thumbSize;
       // public List<WorkData> pakapakaAlbum;
        public List<WorkData> characters;

        public List<WorkData> heads;
        public List<WorkData> bellies;
        public List<WorkData> hands;
        public List<WorkData> feet;

        public List<WorkMetaData> userWorksMetaData;

        string fieldSeparator = ":";
        string itemSeparator = "&";
        string itemFieldSeparator = "#";

        string currentID;

        WorkData currentWork;

        [Serializable]
        public class WorkData
        {
            public bool isPakaPakaArt;
            public string id;
            public Texture2D thumb;
            public PalettesManager.colorNames bgColorName;
            public List<SavedIData> items;

            public Sprite GetSprite()
            {
                return Sprite.Create(thumb, new Rect(0, 0, thumb.width, thumb.height), Vector2.zero);
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
            }
        }

        [Serializable]
        public class WorkMetaData
        {
            public string id;
            public Texture2D thumb;
            public string userID;
        }

        [Serializable]
        public class ServerWorkMetaData {
            public string thumb;
            public string userID;
        }

        private void Start()
        {
            FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
            // PlayerPrefs.|leteAll();
            StartCoroutine(LoadWorks());
        }

        private void OnDestroy() {
            FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
        }

        void OnTokenUpdated() {
            if (Data.Instance.userData.IsLogged()) {
                CancelInvoke();
                LoadUserFilmMetadataFromServer();
            } else
                Invoke("OnTokenUpdated", 1);
        }
        public void LoadUserFilmMetadataFromServer() {
            if (Data.Instance.userData.IsLogged()) {
                userWorksMetaData = new List<WorkMetaData>();
                FirebaseStoryMakerDBManager.Instance.LoadUserWorkMetadataFromServer(OnUserLoadWorkDataFromServer);
            }
        }

        public void SaveWork(Texture2D tex) {
            WorkData wd;
            if (currentID == "" || currentID == null) {
                wd = new WorkData();
                wd.id = "";
            } else
                wd = GetWork(currentID);
                       
            wd.thumb = tex;
            // wd.bgColorName = Data.Instance.palettesManager.GetColorName(UIManager.Instance.boardUI.cam.backgroundColor);
            wd.bgColorName = PalettesManager.colorNames.BLANCO;//To-DO
            wd.items = new List<WorkData.SavedIData>();
            int i = UIManager.Instance.boardUI.items.all.Count;
            int totalParts = 0;
            int partID = 0;
            while (i > 0) {
                ItemInScene iInScene = UIManager.Instance.boardUI.items.all[i - 1];
                int newPartID = (int)iInScene.data.part;
                if (partID != newPartID)
                    totalParts++;
                partID = newPartID;
                if (partID > 0) {
                    WorkData.SavedIData sd = new WorkData.SavedIData();
                    sd.part = partID;
                    sd.id = iInScene.data.id;
                    sd.position = iInScene.data.position;
                    sd.rotation = iInScene.data.rotation;
                    sd.scale = iInScene.data.scale;
                    sd.anim = iInScene.data.anim;
                    sd.color = iInScene.data.colorName;
                    sd.galleryID = iInScene.data.galleryID;
                    wd.items.Add(sd);
                }
                bool mirrorDeleted = UIManager.Instance.boardUI.items.Delete(iInScene);
                if (mirrorDeleted)
                    i--;
                i--;
            }
            currentWork = wd;
            if (wd.id == "") {
                if (totalParts > 1) // is a complete character;
                    characters.Add(wd);
                else // is a part preset;
                    AddPart(partID, wd);
                FirebaseStoryMakerDBManager.Instance.SaveWorkToServer(EncodeWorkData(wd), OnWorkSavedToServer);
            } else
                FirebaseStoryMakerDBManager.Instance.UpdateWorkToServer(wd.id, EncodeWorkData(wd), OnWorkSavedToServer);

            PersistThumbLocal(wd);
           // SetPkpkShared(wd, false);
        }
        string EncodeWorkData(WorkData wd)
        {
                string workData = "";
                workData += Enum.GetName(typeof(PalettesManager.colorNames), wd.bgColorName) + fieldSeparator;
                for (int i = 0; i < wd.items.Count; i++) {
                    workData += wd.items[i].galleryID + itemFieldSeparator + wd.items[i].id + itemFieldSeparator + wd.items[i].position.x + itemFieldSeparator +
                    wd.items[i].position.y + itemFieldSeparator + wd.items[i].position.z + itemFieldSeparator + wd.items[i].rotation.z +
                    itemFieldSeparator + wd.items[i].scale.x +
                    itemFieldSeparator + Enum.GetName(typeof(PalettesManager.colorNames), wd.items[i].color) +
                    itemFieldSeparator + Enum.GetName(typeof(AnimationsManager.anim), wd.items[i].anim) +
                    itemFieldSeparator + wd.items[i].part;
                    if (i < wd.items.Count - 1)
                        workData += itemSeparator;
                }
            Debug.Log("#workData: "+workData);
            return workData;
        }

        void OnWorkSavedToServer(bool succes, string id) {
            currentWork.id = id;
            currentID = id;

            ServerWorkMetaData swmd = new ServerWorkMetaData();
            swmd.thumb = System.Convert.ToBase64String(currentWork.thumb.EncodeToJPG());
            swmd.userID = Data.Instance.userData.userDataInDatabase.uid;
            FirebaseStoryMakerDBManager.Instance.SaveWorkMetadataToServer(currentID, swmd);

            OpenWorkDetail(currentWork);
        }

        void OpenWorkDetail(WorkData wd)
        {
            Sprite sprite = Sprite.Create(wd.thumb, new Rect(0, 0, wd.thumb.width, wd.thumb.height), Vector2.zero);
            UIManager.Instance.workDetailUI.ShowWorkDetail(wd.id, sprite,true);
            Events.ResetItems();
        }

        void PersistThumbLocal(WorkData wd)
        {
            if (wd.id == "")
                wd.id = System.DateTime.Now.ToString("yyyyMMddHHmmss");

            byte[] bytes = wd.thumb.EncodeToPNG();
            string folder = Path.Combine(Application.persistentDataPath, "Thumbs");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string filename = Path.Combine(folder, "thumb_" + wd.id + ".png");

            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("thumb to: {0}", filename));

            OpenWorkDetail(wd);

            PersistWorkDataLocal(wd.id, wd);
        }

        void PersistWorkDataLocal(string id, WorkData wd)
        {
            string workData = "";
            workData += Enum.GetName(typeof(PalettesManager.colorNames), wd.bgColorName) + fieldSeparator;
            for (int i = 0; i < wd.items.Count; i++)
            {
                    workData += wd.items[i].galleryID + itemFieldSeparator + wd.items[i].id + itemFieldSeparator + wd.items[i].position.x + itemFieldSeparator +
                    wd.items[i].position.y + itemFieldSeparator + wd.items[i].position.z + itemFieldSeparator + wd.items[i].rotation.z +
                    itemFieldSeparator + wd.items[i].scale.x +
                    itemFieldSeparator + Enum.GetName(typeof(PalettesManager.colorNames), wd.items[i].color) +
                    itemFieldSeparator + Enum.GetName(typeof(AnimationsManager.anim), wd.items[i].anim) +
                    itemFieldSeparator + wd.items[i].part;
                if (i < wd.items.Count - 1)
                    workData += itemSeparator;
            }

            PlayerPrefs.SetString("Work_" + id, workData);
            PersistWorksIds();
        }

        void PersistWorksIds()
        {
            string workIDs = "";
            foreach (WorkData wd in characters)
                workIDs += wd.id + fieldSeparator;
            foreach (WorkData wd in heads)
                workIDs += wd.id + fieldSeparator;
            foreach (WorkData wd in bellies)
                workIDs += wd.id + fieldSeparator;
            foreach (WorkData wd in hands)
                workIDs += wd.id + fieldSeparator;
            foreach (WorkData wd in feet)
                workIDs += wd.id + fieldSeparator;

            PlayerPrefs.SetString("WorksIds", workIDs);
        }

        public void OnUserLoadWorkDataFromServer(Dictionary<string, ServerWorkMetaData> sfds) {
            foreach (KeyValuePair<string, ServerWorkMetaData> e in sfds) {
                if (userWorksMetaData.Find(x => x.id == e.Key) == null) {
                    WorkMetaData fd = new WorkMetaData();
                    fd.id = e.Key;
                    fd.userID = e.Value.userID;
                    fd.thumb = new Texture2D(1, 1);
                    fd.thumb.LoadImage(System.Convert.FromBase64String(e.Value.thumb));
                    userWorksMetaData.Add(fd);
                }
            }
            FirebaseStoryMakerDBManager.Instance.LoadUserWorksFromServer(LoadWorksFromServer);
        }

        void LoadWorksFromServer(Dictionary<string,string> data) {
            foreach (KeyValuePair<string, string> e in data) {
                Debug.Log("#LoadWorksFromServer "+e.Key+": "+e.Value);
                WorkData wd = new WorkData();
                wd.id = e.Key;
                string[] wData = e.Value.Split(fieldSeparator[0]);
                print("total art: " + wData.Length);
                if (wData[0] != "") {
                    Debug.Log("bgColorIndex: " + wData[0]);
                    wd.bgColorName = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), wData[0]);

                    List<WorkData.SavedIData> items = new List<WorkData.SavedIData>();
                    string[] itemsData = wData[1].Split(itemSeparator[0]);
                    // Debug.Log("ItemCount: " + itemsData.Length);

                    int totalParts = 0;
                    int partID = 0;

                    for (int j = 0; j < itemsData.Length; j++) {
                        string[] iData = itemsData[j].Split(itemFieldSeparator[0]);

                        int num = 0;
                        foreach (string s in iData) {
                            Debug.Log(num + "___ " + s);
                            num++;
                        }
                        //ItemData iD = Data.Instance.galeriasData.GetItem(wd.galleryID,int.Parse(iData[0]));
                        //if (iD.sprite == null)
                        //    Debug.Log(iD.id + ": spriteNull");
                        WorkData.SavedIData sd = new WorkData.SavedIData();
                        Debug.Log("# " + iData[0]);
                        sd.galleryID = int.Parse(iData[0]);
                        sd.id = int.Parse(iData[1]);
                        sd.position = new Vector3(float.Parse(iData[2]), float.Parse(iData[3]), float.Parse(iData[4]));
                        sd.rotation = new Vector3(0f, 0f, float.Parse(iData[5]));
                        sd.scale = new Vector3(float.Parse(iData[6]), float.Parse(iData[6]), 0f);
                        sd.color = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), iData[7]);
                        sd.anim = (AnimationsManager.anim)Enum.Parse(typeof(AnimationsManager.anim), iData[8]);
                        int newPartID = int.Parse(iData[9]);
                        if (newPartID != partID)
                            totalParts++;
                        partID = newPartID;
                        sd.part = partID;
                        // iD.color = Color.white;
                        items.Add(sd);
                    }
                    wd.items = items;
                    
                    wd.thumb = userWorksMetaData.Find(x=>x.id==wd.id).thumb;
                    if (totalParts > 1) //is full character:
                        characters.Add(wd);
                    else //is preset part:
                        AddPart(partID, wd);
                }
            }            
        }

        IEnumerator LoadWorks()
        {
            string[] workIDs = PlayerPrefs.GetString("WorksIds").Split(fieldSeparator[0]);
            for (int i = 0; i < workIDs.Length - 1; i++)
            {
                WorkData wd = new WorkData();
                wd.id = workIDs[i];
                string[] wData = PlayerPrefs.GetString("Work_" + workIDs[i]).Split(fieldSeparator[0]);
                print("total art: " + wData.Length);

                if (wData[0] != "")
                {
                    wd.bgColorName = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), wData[0]);

                    List<WorkData.SavedIData> items = new List<WorkData.SavedIData>();
                    string[] itemsData = wData[1].Split(itemSeparator[0]);

                    int totalParts = 0;
                    int partID = 0;
                    for (int j = 0; j < itemsData.Length; j++)
                    {
                        string[] iData = itemsData[j].Split(itemFieldSeparator[0]);

                        int num = 0;
                        foreach (string s in iData)
                        {
                            Debug.Log(num + "___ " + s);
                            num++;
                        }
                        WorkData.SavedIData sd = new WorkData.SavedIData();
                        sd.galleryID = int.Parse(iData[0]);
                        sd.id = int.Parse(iData[1]);
                        sd.position = new Vector3(float.Parse(iData[2]), float.Parse(iData[3]), float.Parse(iData[4]));
                        sd.rotation = new Vector3(0f, 0f, float.Parse(iData[5]));
                        sd.scale = new Vector3(float.Parse(iData[6]), float.Parse(iData[6]), 0f);
                        sd.color = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), iData[7]);
                        sd.anim = (AnimationsManager.anim)Enum.Parse(typeof(AnimationsManager.anim), iData[8]);
                        int newPartID = int.Parse(iData[9]);
                        if (newPartID != partID)
                            totalParts++;
                        partID = newPartID;
                        sd.part = partID;
                        print("new partID " + partID + " totalParts " + totalParts);
                        items.Add(sd);
                    }
                    wd.items = items;

                    string folder = Path.Combine(Application.persistentDataPath, "Thumbs");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    string filename = Path.Combine(folder, "thumb_" + workIDs[i] + ".png");
                    wd.thumb = TextureUtils.LoadLocal(filename);
                    print("partID " + partID + " totalParts " + totalParts);
                    if (totalParts > 1) //is full character:
                        characters.Add(wd);
                    else //is preset part:
                        AddPart(partID, wd);
                }
            }
            yield return null;
        }

        /*WorkData ParseWork() {

        }*/

        public WorkData GetWork(string id)
        {
            return characters.Find(x => x.id == id);
        }

        public WorkData SetCurrentID(string id)
        {
            currentID = id;
            //  Debug.Log(currentID);
            return characters.Find(x => x.id == id);
        }

        public void ResetCurrentID()
        {
            currentID = "";
        }

        public void DeleteWork(string id)
        {
            WorkData wd = characters.Find(x => x.id == id);
            characters.Remove(wd);
            PlayerPrefs.DeleteKey("Work_" + id);
          //  PlayerPrefs.DeleteKey("PkpkShared_" + id);
            PersistWorksIds();
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
            WorkData wd = characters.Find(x => x.id == id);
            bool hasAnims = false;
            foreach (WorkData.SavedIData item in wd.items)
            {
                if (item.anim != AnimationsManager.anim.NONE)
                {
                    hasAnims = true;
                    return hasAnims;
                }
            }
            return hasAnims;
        }


        void AddPart(int partID, WorkData wd)
        {
            switch (partID)
            {
                case 1: //head
                    heads.Add(wd);
                    break;
                case 2: //belly
                    bellies.Add(wd);
                    break;
                case 3: //head
                    hands.Add(wd);
                    break;
                case 4: //head
                    feet.Add(wd);
                    break;
            }
        }
        public List<WorkData> GetPreset(int partID)
        {
            switch (partID)
            {
                case 1: //head
                    return heads;
                case 2: //belly
                    return bellies;
                case 3: //head
                    return hands;
                default: //head
                    return feet;
            }
        }

    }

}