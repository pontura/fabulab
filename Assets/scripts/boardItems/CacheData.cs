using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;
using BoardItems;

public class CacheData : MonoBehaviour
{
    public Dictionary<string, List<SceneData>> filmsCache;
    public List<userData> users;

    [SerializeField] List<SceneData> filmData;

    [Serializable]
    public class userData {
        public string userId;
        public string username;        
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

    public void GetUserName(string uId, System.Action<string> callback) {
        userData udata = users.Find(x => x.userId == uId);
        if (udata != null)
            callback(udata.username);
        else
            FirebaseStoryMakerDBManager.Instance.GetUsernameFromServer(uId, OnGetUserNameFromServer);
    }

    void OnGetUserNameFromServer(string uid, string uname) {
        userData udata = new userData();
        udata.userId = uid;
        udata.username = uname;
        users.Add(udata);
        //Events.OnGetUserName(uname);
    }
    
}
