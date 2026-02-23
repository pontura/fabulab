using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.StoryMaker.DB;
using Yaguar.StoryMaker.Editor;

namespace BoardItems
{
    public class ScenesData : MonoBehaviour {
        string sdSeparator = "|";
        string fieldSeparator = "#";
        public List<FilmDataFabulab> userFilmsData;
        public List<FilmDataFabulab> filmsData;
        public FilmDataFabulab currentFilmData;
        // public string currentName;

        public bool loadedDone;

        [Serializable]
        public class FilmDataFabulab : Yaguar.StoryMaker.Editor.FilmData
        {
            public int likes;
            public bool IsMyStory() {
                if (userID == null || (Data.Instance.userData.userDataInDatabase != null && userID == Data.Instance.userData.userDataInDatabase.uid))
                    return true;
                return false;
            }
        }

        [Serializable]
        public class ServerFilmData
        {
            public string thumb;
            public string name;
            public string userID;
            public int likes;
            public int speed;
        }

        private void Start() {
            //Events.OnThemesLoadedComplete += LoadThemeFilmMetadataFromServer;
            FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
            StoryMakerEvents.ChangeSpeed += ChangeSpeed;
            StoryMakerEvents.Restart += Init;

            if (!Data.Instance.userData.IsLogged())
                LoadFilmMetadataLocal();
            Init();
        }

        private void Init() {
            ScenesManager.Instance.Init();
            if (Data.Instance.userData.IsLogged())
                ScenesManager.Instance.currentFDataID = "";
            else if (userFilmsData.Count > 1) {
                ScenesManager.Instance.currentFDataID = "" + (int.Parse(userFilmsData.Max(t => t.id)) + 1);
            }

        }
        private void OnDestroy() {
            //Events.OnThemesLoadedComplete -= LoadUserFilmMetadataFromServer;
            FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
            StoryMakerEvents.ChangeSpeed -= ChangeSpeed;
            StoryMakerEvents.Restart -= Init;
        }
        void ChangeSpeed(int speed) {
            if (currentFilmData != null) {
                currentFilmData.speed = speed;
                ScenesManager.Instance.currentFilmData = currentFilmData;
            }
        }

        public void StartNewStory(string storyName) {
            Debug.Log("#StartNewStory");
            Init();
            ScenesManager.Instance.Restart();
            //hace un nuevo id unico:
            ScenesManager.Instance.currentFDataID = UnityEngine.Random.Range(0, 1000).ToString();
            bool idExists = false;
            foreach (FilmDataFabulab filmData in userFilmsData) {
                if (ScenesManager.Instance.currentFDataID == filmData.id)
                    idExists = true;
            }
            currentFilmData = new FilmDataFabulab();
            currentFilmData.name = storyName;
            ScenesManager.Instance.currentFilmData = currentFilmData;
            if (idExists) {
                Debug.Log("#StartNewStory idExists");
                StartNewStory(storyName);
            }

            loadedDone = true;
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
                ScenesManager.Instance.currentFDataID = "";
                userFilmsData = new List<FilmDataFabulab>();
                FirebaseStoryMakerDBManager.Instance.LoadUserFilmDataFromServer(OnUserAddFilmDataFromServer);
            }
            SortUserFilmsDataByLikes();
        }

        void LoadFilmMetadataLocal() {
            ScenesManager.Instance.currentFDataID = "0";
            userFilmsData = new List<FilmDataFabulab>();
            string fIds = PlayerPrefs.GetString("Films_ids");
            if (fIds.Length > 0) {
                string[] arr = fIds.Split(sdSeparator[0]);
                for (int i = 0; i < arr.Length - 1; i++) {
                    string[] arr2 = arr[i].Split(fieldSeparator[0]);
                    //filmData += fid.id + fieldSeparator + fid.speed + fieldSeparator + fid.name + fid.framecount + sdSeparator;
                    FilmDataFabulab fd = new FilmDataFabulab();
                    fd.id = arr2[0];
                    fd.speed = int.Parse(arr2[1]);
                    fd.name = arr2[2];
                    fd.framecount = int.Parse(arr2[3]);
                    string folder = Path.Combine(Application.persistentDataPath, "Thumbs");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    string filename = Path.Combine(folder, "movie_thumb_" + fd.id + ".png");
                    fd.thumb = TextureUtils.LoadLocal(filename);
                    userFilmsData.Add(fd);
                }
            }
        }

        int lynaLikeCount = 0;
        int pageLimitFactor = 1;

        public void UpdateCurrentName(string name) {
            currentFilmData.name = name;
            ScenesManager.Instance.currentFilmData = currentFilmData;
        }

        public void RemoveFD(string id) {
            userFilmsData.RemoveAll(x => x.id == id);
            filmsData.RemoveAll(x => x.id == id);
        }

        public void OnUserAddFilmDataFromServer(Dictionary<string, ServerFilmData> sfds) {
            foreach (KeyValuePair<string, ServerFilmData> e in sfds) {
                if (userFilmsData.Find(x => x.id == e.Key) == null) {
                    FilmDataFabulab fd = new FilmDataFabulab();
                    fd.id = e.Key;
                    fd.framecount = -1;
                    fd.likes = e.Value.likes;
                    fd.name = e.Value.name;
                    fd.userID = e.Value.userID;
                    fd.speed = e.Value.speed;
                    fd.thumb = new Texture2D(1, 1);
                    fd.thumb.LoadImage(System.Convert.FromBase64String(e.Value.thumb));
                    userFilmsData.Add(fd);
                }
            }
            //Events.OnUpdateFilmIcon();
        }

        public void OnAddFilmDataFromServer(Dictionary<string, ServerFilmData> usfds, int pageId) {
            if (usfds != null) {
                if (pageId == 0)
                    filmsData = new List<FilmDataFabulab>();
                Debug.Log("Load Theme Film Data from Server");
                foreach (KeyValuePair<string, ServerFilmData> e in usfds) {
                    FilmDataFabulab fd = new FilmDataFabulab();
                    fd.id = e.Key;
                    fd.framecount = -1;
                    fd.likes = e.Value.likes;
                    fd.name = e.Value.name;
                    fd.userID = e.Value.userID;
                    fd.speed = e.Value.speed;
                    fd.thumb = new Texture2D(1, 1);
                    fd.thumb.LoadImage(System.Convert.FromBase64String(e.Value.thumb));
                    filmsData.Add(fd);
                }
                SortThemeFilmsDataByLikes(true);
                //Data.Instance.cache.AddToThemeFilmsDataToCache(Data.Instance.themesManager.selectedTheme.id, filmsData, showType, pageId);
                //Events.OnUpdateFilmIcon();
            }
            Events.OnLoading(false);
        }

        public void SortThemeFilmsDataByLikes(bool moreLikes) {
            if (moreLikes)
                filmsData = filmsData.OrderByDescending(x => x.likes).ToList();
            else
                filmsData = filmsData.OrderBy(x => x.likes).ToList();
        }

        public void SortUserFilmsDataByLikes() {
            userFilmsData = userFilmsData.OrderByDescending(x => x.likes).ToList();
        }

        public void OnFilmDataFromServerCompleted() {
            ScenesManager.Instance.currentFDataID = "";
        }



        bool FilmExists() {
            bool exists = false;
            foreach (FilmDataFabulab filmData in userFilmsData) {
                if (ScenesManager.Instance.currentFDataID == filmData.id)
                    exists = true;
            }
            return exists;
        }
        public void SaveFilm() {
            Debug.Log("# SaveFilm");
            StoryMakerEvents.OnSaveScene();
            //Data.Instance.firebaseAuthManager.SaveFilmToServer(ScenesManager.Instance.currentFDataID, scenes);
            if (Data.Instance.userData.IsLogged()) {
                if (!FilmExists()) {
                    FirebaseStoryMakerDBManager.Instance.SaveFilmToServer(ScenesManager.Instance.scenes, OnFilmSavedToServer);
                } else
                    FirebaseStoryMakerDBManager.Instance.UpdateFilmToServer(ScenesManager.Instance.currentFDataID, ScenesManager.Instance.scenes, OnFilmSavedToServer);
            } else {
                for (int i = 0; i < ScenesManager.Instance.scenes.Count; i++) {

                    PlayerPrefs.SetString("FilmId_" + ScenesManager.Instance.currentFDataID + "_" + i, ScenesManager.Instance.scenes[i].Serialize());

                    if (ScenesManager.Instance.currentSceneId == 1)
                        Invoke("SaveTexture", 3 * Time.deltaTime);
                    else
                        SaveTexture();
                }
            }
        }

        void OnFilmSavedToServer(bool succes, string id) {
            ScenesManager.Instance.currentFDataID = id;
            //Data.Instance.cache.AddToFilmCache(ScenesManager.Instance.currentFDataID, ScenesManager.Instance.scenes);
            SaveTexture();
        }

        void SaveTexture() {
            FilmDataFabulab fd = userFilmsData.Find(x => x.id == ScenesManager.Instance.currentFDataID);
            if (fd == null) {
                fd = new FilmDataFabulab();
                fd.id = ScenesManager.Instance.currentFDataID;
                fd.framecount = ScenesManager.Instance.scenes.Count;
                fd.thumb = currentFilmData.thumb;
                fd.name = currentFilmData.name;
                fd.speed = currentFilmData.speed;
                userFilmsData.Add(fd);
            } else {
                fd.framecount = ScenesManager.Instance.scenes.Count;
                fd.thumb = currentFilmData.thumb;
                fd.name = currentFilmData.name;
                fd.speed = currentFilmData.speed;
            }
            currentFilmData = fd;

            ScenesManager.Instance.currentFilmData = currentFilmData;

            byte[] bytes = fd.thumb.EncodeToPNG();

            if (Data.Instance.userData.IsLogged()) {
                FilmDataFabulab tfd = filmsData.Find(x => x.id == fd.id);
                if (tfd == null)
                    filmsData.Add(fd);
                else
                    tfd = fd;

                ServerFilmData sfd = new ServerFilmData();
                sfd.thumb = System.Convert.ToBase64String(bytes);
                sfd.name = fd.name;
                sfd.speed = fd.speed;
                sfd.userID = Data.Instance.userData.userDataInDatabase.uid;
                FirebaseStoryMakerDBManager.Instance.SaveFilmDataToServer(fd.id, sfd);
            } else {
                UpdateLocalFilmData();

                string folder = Path.Combine(Application.persistentDataPath, "Thumbs");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                string filename = Path.Combine(folder, "movie_thumb_" + fd.id + ".png");

                System.IO.File.WriteAllBytes(filename, bytes);
                Debug.Log(string.Format("thumb to: {0}", filename));
            }
            //Events.OnUpdateFilmIcon();
        }

        void UpdateLocalFilmData() {
            string filmData = "";
            foreach (FilmDataFabulab fid in userFilmsData)
                filmData += fid.id + fieldSeparator + fid.speed + fieldSeparator + fid.name + fieldSeparator + fid.framecount + sdSeparator;
            if (filmData != "")
                PlayerPrefs.SetString("Films_ids", filmData);
            else
                PlayerPrefs.DeleteKey("Films_ids");
        }

        public void LoadUserFilm(string _id) {
            print("____________LoadUserFilm" + loadedDone);
            loadedDone = false;
            currentFilmData = userFilmsData.Find(x => x.id == _id);
            ScenesManager.Instance.currentFilmData = currentFilmData;
            if (currentFilmData != null) {
                ScenesManager.Instance.currentFDataID = currentFilmData.id;
                ScenesManager.Instance.scenes = new List<SceneData>();

                if (Data.Instance.userData.IsLogged()) {
                    Events.OnLoading(true);
                    //Data.Instance.firebaseAuthManager.LoadFilmFromServer(ScenesManager.Instance.currentFDataID, 0);
                    if (Data.Instance.cacheData.filmsCache.ContainsKey(ScenesManager.Instance.currentFDataID)) {
                        Debug.Log("Load From Cache");
                        ScenesManager.Instance.scenes = Data.Instance.cacheData.GetCacheFilmData(ScenesManager.Instance.currentFDataID);
                        LoadSucces();
                    } else
                        FirebaseStoryMakerDBManager.Instance.LoadFilmFromServer(currentFilmData, OnFilmLoadedFromServer);
                } else {
                    LoadScenesFromLocal(currentFilmData);
                    loadedDone = true;
                }
            }
        }

        void LoadScenesFromLocal(FilmDataFabulab fd) {
            for (int i = 0; i < fd.framecount; i++) {
                SceneData sd = new SceneData();
                sd.Deserialize(PlayerPrefs.GetString("FilmId_" + fd.id + "_" + i));
                ScenesManager.Instance.scenes.Add(sd);
            }
        }

        void LoadSucces() {
            //Debug.Log("____________LoadSucces");
            InitLoadedFilm();
            Events.OnLoading(false);
            loadedDone = true;
        }

        void OnFilmLoadedFromServer(bool succes, SceneData[] sds) {
            if (succes) {
                ScenesManager.Instance.scenes = sds.OfType<SceneData>().ToList();
                Data.Instance.cacheData.AddToFilmCache(ScenesManager.Instance.currentFDataID, ScenesManager.Instance.scenes);
                LoadSucces();
            }
        }


        public void InitLoadedFilm() {
            if (ScenesManager.Instance.scenes.Count > 0 && ScenesManager.Instance.scenes.Count >= currentFilmData.framecount) {
                ScenesManager.Instance.currentSceneId = 1;
                ScenesManager.Instance.AddSceneObjectsToScene();
                StoryMakerEvents.OnLoadFilm();
            } else {
                Invoke("InitLoadedFilm", 1);
            }
        }
    }
           
}
