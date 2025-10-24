using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using BoardItems.UI;
using BoardItems.Characters;

namespace BoardItems
{
    public class AlbumData : MonoBehaviour
    {
        public Vector2Int thumbSize;
        public List<WorkData> pakapakaAlbum;
        public List<WorkData> album;

        string fieldSeparator = ":";
        string itemSeparator = "&";
        string itemFieldSeparator = "#";

        string currentID;

        [Serializable]
        public class WorkData
        {
            public bool isPakaPakaArt;
            public string id;
            public Texture2D thumb;
            public PalettesManager.colorNames bgColorName;
            public List<SavedIData> items;
            public bool pkpkShared;

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

        private void Start()
        {
            // PlayerPrefs.DeleteAll();
            StartCoroutine(LoadWorks());
        }

        public void SaveWork(Texture2D tex)
        {
            WorkData wd;
            if (currentID == "" || currentID == null)
            {
                wd = new WorkData();
                wd.id = "";
            }
            else
                wd = GetWork(currentID);


            wd.thumb = tex;
            // wd.bgColorName = Data.Instance.palettesManager.GetColorName(UIManager.Instance.boardUI.cam.backgroundColor);
            wd.bgColorName = PalettesManager.colorNames.BLANCO;//To-DO
            wd.items = new List<WorkData.SavedIData>();
            int i = UIManager.Instance.boardUI.items.all.Count;
            while(i>0)
            {
                ItemInScene iInScene = UIManager.Instance.boardUI.items.all[i-1];

                int partID = (int)iInScene.data.part;
                if (partID > 0)
                {
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
                if(mirrorDeleted)
                    i--;
                i--;
            }
            if (wd.id == "")
                album.Add(wd);
            PersistThumbLocal(wd);
            SetPkpkShared(wd, false);
        }

        void OpenWorkDetail(WorkData wd)
        {
            Sprite sprite = Sprite.Create(wd.thumb, new Rect(0, 0, wd.thumb.width, wd.thumb.height), Vector2.zero);
            UIManager.Instance.workDetailUI.ShowWorkDetail(wd.id, sprite, wd.pkpkShared, true);
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
            foreach (WorkData wd in album)
                workIDs += wd.id + fieldSeparator;

            PlayerPrefs.SetString("WorksIds", workIDs);
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
                    Debug.Log("bgColorIndex: " + wData[0]);
                    wd.bgColorName = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), wData[0]);

                    List<WorkData.SavedIData> items = new List<WorkData.SavedIData>();
                    string[] itemsData = wData[1].Split(itemSeparator[0]);
                    // Debug.Log("ItemCount: " + itemsData.Length);
                    for (int j = 0; j < itemsData.Length; j++)
                    {
                        string[] iData = itemsData[j].Split(itemFieldSeparator[0]);

                        int num = 0;
                        foreach (string s in iData)
                        {
                            Debug.Log(num + "___ " + s);
                            num++;
                        }
                        //ItemData iD = Data.Instance.galeriasData.GetItem(wd.galleryID,int.Parse(iData[0]));
                        //if (iD.sprite == null)
                        //    Debug.Log(iD.id + ": spriteNull");
                        WorkData.SavedIData sd = new WorkData.SavedIData();
                        sd.galleryID = int.Parse(iData[0]);
                        sd.id = int.Parse(iData[1]);
                        sd.position = new Vector3(float.Parse(iData[2]), float.Parse(iData[3]), float.Parse(iData[4]));
                        sd.rotation = new Vector3(0f, 0f, float.Parse(iData[5]));
                        sd.scale = new Vector3(float.Parse(iData[6]), float.Parse(iData[6]), 0f);
                        sd.color = (PalettesManager.colorNames)Enum.Parse(typeof(PalettesManager.colorNames), iData[7]);
                        sd.anim = (AnimationsManager.anim)Enum.Parse(typeof(AnimationsManager.anim), iData[8]);
                        sd.part = int.Parse(iData[9]);
                        // iD.color = Color.white;
                        items.Add(sd);
                    }
                    wd.items = items;

                    string folder = Path.Combine(Application.persistentDataPath, "Thumbs");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    string filename = Path.Combine(folder, "thumb_" + workIDs[i] + ".png");
                    wd.thumb = TextureUtils.LoadLocal(filename);
                    wd.pkpkShared = PlayerPrefs.GetInt("PkpkShared_" + workIDs[i]) == 1;
                    album.Add(wd);
                }
            }

            yield return null;
        }

        public WorkData GetWork(string id)
        {
            return album.Find(x => x.id == id);
        }

        public WorkData SetCurrentID(string id)
        {
            currentID = id;
            //  Debug.Log(currentID);
            return album.Find(x => x.id == id);
        }

        public void ResetCurrentID()
        {
            currentID = "";
        }

        public void DeleteWork(string id)
        {
            WorkData wd = album.Find(x => x.id == id);
            album.Remove(wd);
            PlayerPrefs.DeleteKey("Work_" + id);
            PlayerPrefs.DeleteKey("PkpkShared_" + id);
            PersistWorksIds();
        }

        public void SetPkpkShared(string id, bool enable)
        {
            WorkData wd = album.Find(x => x.id == id);
            SetPkpkShared(wd, enable);
        }

        public void SetPkpkShared(WorkData wd, bool enable)
        {
            wd.pkpkShared = enable;
            PlayerPrefs.SetInt("PkpkShared_" + wd.id, enable ? 1 : 0);
            UIManager.Instance.workDetailUI.SetSendedSign(wd.id, enable);
        }

        public bool IsPkpkShared(string id)
        {
            return album.Find(x => x.id == id).pkpkShared;
        }

        public bool HasAnims(string id)
        {
            WorkData wd = album.Find(x => x.id == id);
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

    }

}