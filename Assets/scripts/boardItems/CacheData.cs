using BoardItems;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;

public class CacheData : MonoBehaviour
{
    public Dictionary<string, List<SceneDataFabulab>> filmsCache;
    public List<UserData> users;

    [SerializeField] List<SceneDataFabulab> filmData;

    [SerializeField] private int maxUnusedDays = 30; // tiempo de vida si no se usa
    private string cacheDir;
    

    [Serializable]
    public class ServerMetaData
    {
        public string thumb_timestamp;
        public string username;
    }

    [Serializable]
    public class UserData {
        public string uid;
        public string username;
        public string thumb_timestamp;
        //public Texture2D thumb;
    }

    [Serializable]
    public class FilmsDataCache {
        public string id;
        public List<FilmDataCachePage> filmsDataPage;
    }

    [Serializable]
    public class FilmDataCachePage {
        public int id;
        public List<FilmDataFabulab> filmsData;
    }

    [Serializable]
    public class ImageMeta
    {
        public string serverTimestamp; // timestamp del server
        public long lastAccess;      // timestamp local de último uso
    }

    // Start is called before the first frame update
    void Start()
    {
        filmsCache = new Dictionary<string, List<SceneDataFabulab>>();
        cacheDir = Application.persistentDataPath + "/imageCache";
        CleanCache();
    }            

    public void AddToFilmCache(string id, List<SceneDataFabulab> source) {
    //   Debug.Log("AddToFilmCache");
        if(filmsCache.ContainsKey(id))
            filmsCache[id] =  CloneFilmData(source);
        else
            filmsCache.Add(id, CloneFilmData(source));
    }

    public void RemoveFilmCache(string id) {
        filmsCache.Remove(id);
    }

    List<SceneDataFabulab> CloneFilmData(List<SceneDataFabulab> source) {
        List<SceneDataFabulab> nueva = new List<SceneDataFabulab>();
        foreach (SceneDataFabulab item in source)
            nueva.Add(item.Clone());
        return nueva;
    }    

    public List<SceneDataFabulab> GetCacheFilmData(string id) {
        filmData = filmsCache[id];
        return CloneFilmData(filmsCache[id]);
    }
    public void GetUser(string uid, System.Action<UserData, Texture2D> OnReady)
    {
        //print("GET user: " + uid);
        foreach (UserData u in users)
        {
            if (u.uid == uid)
            {
                LoadImage("profile", uid, (tex) => OnReady(u, tex), u.thumb_timestamp,userId: uid);
                return;
            }
        }
        FirebaseStoryMakerDBManager.Instance.GetUser(uid, (uid, rawjson) =>
        {
            UserData ud = users.Find(x=>x.uid == uid);
            if (ud != null) {
                LoadImage("profile", uid, (tex) => OnReady(ud, tex), ud.thumb_timestamp, userId: uid);
                return;
            }
            //Debug.Log("# " + uid + " " + rawjson);
            ServerMetaData s = JsonUtility.FromJson<ServerMetaData>(rawjson);
            Debug.Log("# " + uid + " " + rawjson + " " + s.thumb_timestamp);
            ud = new UserData();
            ud.uid = uid;
            ud.username = s.username;
            ud.thumb_timestamp = s.thumb_timestamp;
            if (string.IsNullOrEmpty(ud.thumb_timestamp)) {
                ud.thumb_timestamp = DateTime.MinValue.ToUniversalTime().ToString("o");
            }
            /*ud.thumb = new Texture2D(1, 1);
            if(s.thumb != "" && s.thumb != null)
                ud.thumb.LoadImage(System.Convert.FromBase64String(s.thumb));*/            
            users.Add(ud);
            LoadImage("profile", uid, (tex) => OnReady(ud, tex), ud.thumb_timestamp, userId: uid);
        });
    }

    public void LoadImage(string folder, string id, Action<Texture2D> onComplete, string serverTimestamp, string userId = null) {

        if(serverTimestamp==null)
            serverTimestamp = DateTime.Now.ToUniversalTime().ToString("o");

        string folderPath = cacheDir + "/" + folder;
        Directory.CreateDirectory(folderPath);
        string path = Path.Combine(folderPath, $"{id}.jpg");
        string metaPath = Path.Combine(folderPath, $"{id}.meta");        

        ImageMeta meta = null;
        if (File.Exists(metaPath)) {
            meta = JsonUtility.FromJson<ImageMeta>(File.ReadAllText(metaPath));
        }

        bool needDownload = true;

        if (File.Exists(path) && meta != null) {
            
            if (!(String.Compare(meta.serverTimestamp, serverTimestamp) < 0)) { 
                byte[] fileContents = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileContents);

                // Actualizar último acceso
                meta.lastAccess = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                File.WriteAllText(metaPath, JsonUtility.ToJson(meta));

                onComplete?.Invoke(tex);
                needDownload = false;
                Debug.Log($"% Loaded ImageCache from {path}");
            }
        }

        if (needDownload) {
            FirebaseStoryMakerDBManager.Instance.DownloadTexture(folder, id, onComplete, serverTimestamp, userId);            
        }
    }

    public void SaveImageCache(string folder, string filename, byte[] fileContents, string serverTimestamp) {
        string folderPath = cacheDir + "/" + folder;
        Directory.CreateDirectory(folderPath);
        string path = Path.Combine(folderPath, $"{filename}.jpg");
        string metaPath = Path.Combine(folderPath, $"{filename}.meta");

        Debug.Log($"% SaveImageCache to {path}");
        File.WriteAllBytes(path, fileContents);
       
        // Guardar meta actualizado
        ImageMeta meta = meta = new ImageMeta {
            serverTimestamp = serverTimestamp,
            lastAccess = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        File.WriteAllText(metaPath, JsonUtility.ToJson(meta));
    }

    public void CleanCache() {
        Debug.Log("& CleanCache");
        foreach (BoardItems.BoardData.MetadataTypes mdTypes in Enum.GetValues(typeof(BoardItems.BoardData.MetadataTypes))) {
            CheckAndCleanFolderCache(mdTypes.ToString());            
        }
        CheckAndCleanFolderCache("profile");
    }

    void CheckAndCleanFolderCache(string folderName) {
        string folderPath = cacheDir + "/" + folderName;
        Directory.CreateDirectory(folderPath);
        foreach (var metaFile in Directory.GetFiles(folderPath, "*.meta")) {
            ImageMeta meta = JsonUtility.FromJson<ImageMeta>(File.ReadAllText(metaFile));
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            //Debug.Log($"& meta last access: {meta.lastAccess} now: {now}");
            if ((now - meta.lastAccess) > maxUnusedDays * 86400) {
                Debug.Log($"& metafile to delete: {metaFile}");
                string imgFile = Path.ChangeExtension(metaFile, ".jpg");
                if (File.Exists(imgFile)) File.Delete(imgFile);
                File.Delete(metaFile);
            }
        }
    }


}
