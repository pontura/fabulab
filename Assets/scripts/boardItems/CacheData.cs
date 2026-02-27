using BoardItems;
using Firebase.Analytics;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;

public class CacheData : MonoBehaviour
{
    public Dictionary<string, List<SceneData>> filmsCache;
    public List<UserData> users;

    [SerializeField] List<SceneData> filmData;

    [Serializable]
    public class ServerMetaData
    {
        public string thumb;
        public string username;
    }

    [Serializable]
    public class UserData {
        public string uid;
        public string username;
        public Texture2D thumb;
    }

    [Serializable]
    public class FilmsDataCache {
        public string id;
        public List<FilmDataCachePage> filmsDataPage;
    }

    [Serializable]
    public class FilmDataCachePage {
        public int id;
        public List<ScenesData.FilmDataFabulab> filmsData;
    }

    // Start is called before the first frame update
    void Start()
    {
        filmsCache = new Dictionary<string, List<SceneData>>();
    }            

    public void AddToFilmCache(string id, List<SceneData> source) {
     //   Debug.Log("AddToFilmCache");
        if(filmsCache.ContainsKey(id))
            filmsCache[id] =  CloneFilmData(source);
        else
            filmsCache.Add(id, CloneFilmData(source));
    }

    List<SceneData> CloneFilmData(List<SceneData> source) {
        List<SceneData> nueva = new List<SceneData>(source.Count);
        foreach (SceneData item in source)
            nueva.Add(item.Clone());
        return nueva;
    }    

    public List<SceneData> GetCacheFilmData(string id) {
        filmData = filmsCache[id];
        return CloneFilmData(filmsCache[id]);
    }
    public void GetUser(string uid, System.Action<UserData> OnReady)
    {
        print("GET user: " + uid);
        foreach (UserData u in users)
        {
            if (u.uid == uid)
            {
                OnReady( u);
            }
        }
        FirebaseStoryMakerDBManager.Instance.GetUser(uid, (uid, rawjson) =>
        {
            ServerMetaData s = JsonUtility.FromJson<ServerMetaData>(rawjson);
            UserData ud = new UserData();
            ud.uid = uid;

            ud.thumb = new Texture2D(1, 1);
            if(s.thumb != "")
                ud.thumb.LoadImage(System.Convert.FromBase64String(s.thumb));
            foreach (UserData u in users)
            {
                if (u.uid == uid)
                {
                    OnReady(u);
                    return;
                }
            }
            users.Add(ud);
            OnReady(ud);
        });
    }
  
    
}
