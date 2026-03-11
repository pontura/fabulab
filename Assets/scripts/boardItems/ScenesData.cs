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

    public class ScenesData : MonoBehaviour {
        string sdSeparator = "|";
        string fieldSeparator = "#";
        public List<FilmDataFabulab> userFilmsData;
        public List<FilmDataFabulab> filmsData;
        public FilmDataFabulab currentFilmData;
        // public string currentName;

        public bool loadedDone;

        

        private void Start() {
            //Events.OnThemesLoadedComplete += LoadThemeFilmMetadataFromServer;
            FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
            StoryMakerEvents.ChangeSpeed += ChangeSpeed;
            StoryMakerEvents.Restart += Restart;

            if (!Data.Instance.userData.IsLogged())
                LoadFilmMetadataLocal();

            Init();
        }

        void Init() {
            Restart();
        }

        private void Restart() {
            ScenesManagerFabulab.Instance.Init();
            if (Data.Instance.userData.IsLogged())
                ScenesManagerFabulab.Instance.currentFDataID = "";
            else if (userFilmsData.Count > 1) {
                ScenesManagerFabulab.Instance.currentFDataID = "" + (int.Parse(userFilmsData.Max(t => t.id)) + 1);
            }

        }
        private void OnDestroy() {
            //Events.OnThemesLoadedComplete -= LoadUserFilmMetadataFromServer;
            FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
            StoryMakerEvents.ChangeSpeed -= ChangeSpeed;
            StoryMakerEvents.Restart -= Restart;
        }
        void ChangeSpeed(int speed) {
            if (currentFilmData != null) {
                currentFilmData.speed = speed;
                ScenesManagerFabulab.Instance.currentFilmData = currentFilmData;
            }
        }

        public void StartNewStory(string storyName) {
            Debug.Log("#StartNewStory");
            Restart();
            ScenesManagerFabulab.Instance.Restart();
            //hace un nuevo id unico:
            ScenesManagerFabulab.Instance.currentFDataID = UnityEngine.Random.Range(0, 1000).ToString();
            bool idExists = false;
            foreach (FilmDataFabulab filmData in userFilmsData) {
                if (ScenesManagerFabulab.Instance.currentFDataID == filmData.id)
                    idExists = true;
            }
            currentFilmData = new FilmDataFabulab();
            currentFilmData.name = storyName;
            ScenesManagerFabulab.Instance.currentFilmData = currentFilmData;
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
                ScenesManagerFabulab.Instance.currentFDataID = "";
                userFilmsData = new List<FilmDataFabulab>();
                FirebaseStoryMakerDBManager.Instance.LoadUserFilmDataFromServer(OnUserAddFilmDataFromServer);
            }
            SortUserFilmsDataByLikes();
        }

        void LoadFilmMetadataLocal() {
            ScenesManagerFabulab.Instance.currentFDataID = "0";
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
            ScenesManagerFabulab.Instance.currentFilmData = currentFilmData;
        }

        public void RemoveFD(string id) {
            userFilmsData.RemoveAll(x => x.id == id);
            filmsData.RemoveAll(x => x.id == id);
        }

        public void OnUserAddFilmDataFromServer(Dictionary<string, ServerFilmData> sfds) {
            if(sfds==null) return;

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
            ScenesManagerFabulab.Instance.currentFDataID = "";
        }



        bool FilmExists() {
            bool exists = false;
            foreach (FilmDataFabulab filmData in userFilmsData) {
                if (ScenesManagerFabulab.Instance.currentFDataID == filmData.id)
                    exists = true;
            }
            return exists;
        }
        public void SaveFilm() {
            Debug.Log("# SaveFilm");
            StoryMakerEvents.OnSaveScene();
            //Data.Instance.firebaseAuthManager.SaveFilmToServer(ScenesManagerFabulab.Instance.currentFDataID, scenes);
            if (Data.Instance.userData.IsLogged()) {
                if (!FilmExists()) {
                    //Debug.Log(ScenesManagerFabulab.Instance.GetSerialized());
                    FirebaseStoryMakerDBManager.Instance.SaveFilmToServer(ScenesManagerFabulab.Instance.GetSerialized(), OnFilmSavedToServer);
                } else
                    FirebaseStoryMakerDBManager.Instance.UpdateFilmToServer(ScenesManagerFabulab.Instance.currentFDataID, ScenesManagerFabulab.Instance.GetSerialized(), OnFilmSavedToServer);
            }
        }

        void OnFilmSavedToServer(bool succes, string id) {
            Debug.Log("& OnFilmSavedToServer");
            ScenesManagerFabulab.Instance.currentFDataID = id;
            Data.Instance.cacheData.AddToFilmCache(ScenesManagerFabulab.Instance.currentFDataID, ScenesManagerFabulab.Instance.Scenes);
            SaveTexture();
        }

        void SaveTexture() {
            Debug.Log("& SaveTexture");
            FilmDataFabulab fd = userFilmsData.Find(x => x.id == ScenesManagerFabulab.Instance.currentFDataID);
            if (fd == null) {
                fd = new FilmDataFabulab();
                fd.id = ScenesManagerFabulab.Instance.currentFDataID;
                fd.framecount = ScenesManagerFabulab.Instance.Scenes.Count;
                fd.thumb = currentFilmData.thumb;
                fd.name = currentFilmData.name;
                fd.speed = currentFilmData.speed;
                userFilmsData.Add(fd);
            } else {
                fd.framecount = ScenesManagerFabulab.Instance.Scenes.Count;
                fd.thumb = currentFilmData.thumb;
                fd.name = currentFilmData.name;
                fd.speed = currentFilmData.speed;
            }
            currentFilmData = fd;

            ScenesManagerFabulab.Instance.currentFilmData = currentFilmData;

            byte[] bytes = fd.thumb.EncodeToPNG();

            if (Data.Instance.userData.IsLogged()) {
                FilmDataFabulab tfd = filmsData.Find(x => x.id == fd.id);
                if (tfd == null)
                    filmsData.Add(fd);
                else
                    tfd = fd;

                ServerFilmData sfd = new ServerFilmData();
                sfd.thumb = System.Convert.ToBase64String(bytes);
                Debug.Log("& Length: " + sfd.thumb.Length);
                sfd.name = fd.name;
                sfd.speed = fd.speed;
                sfd.userID = Data.Instance.userData.userDataInDatabase.uid;
                FirebaseStoryMakerDBManager.Instance.SaveFilmDataToServer(fd.id, sfd);
            }
            //Events.OnUpdateFilmIcon();
        }        

        public void LoadUserFilm(string _id) {
            print("____________LoadUserFilm" + loadedDone);
            loadedDone = false;
            currentFilmData = userFilmsData.Find(x => x.id == _id);
            ScenesManagerFabulab.Instance.currentFilmData = currentFilmData;
            if (currentFilmData != null) {
                ScenesManagerFabulab.Instance.currentFDataID = currentFilmData.id;
                ScenesManagerFabulab.Instance.Scenes = new List<SceneDataFabulab>();

                if (Data.Instance.userData.IsLogged()) {
                    Events.OnLoading(true);
                    //Data.Instance.firebaseAuthManager.LoadFilmFromServer(ScenesManagerFabulab.Instance.currentFDataID, 0);
                    if (Data.Instance.cacheData.filmsCache.ContainsKey(ScenesManagerFabulab.Instance.currentFDataID)) {
                        Debug.Log("Load From Cache");
                        ScenesManagerFabulab.Instance.Scenes = Data.Instance.cacheData.GetCacheFilmData(ScenesManagerFabulab.Instance.currentFDataID);
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
                SceneDataFabulab sd = new SceneDataFabulab();
                sd.Deserialize(PlayerPrefs.GetString("FilmId_" + fd.id + "_" + i));
                ScenesManagerFabulab.Instance.Scenes.Add(sd);
            }
        }

        void LoadSucces() {
            //Debug.Log("____________LoadSucces");
            InitLoadedFilm();
            Events.OnLoading(false);
            loadedDone = true;
        }

        void OnFilmLoadedFromServer(bool succes, List<SceneDataFabulab> sds) {
            if (succes) {
                //ScenesManagerFabulab.Instance.Scenes = sds.OfType<SceneDataFabulab>().ToList();
                ScenesManagerFabulab.Instance.Scenes = sds;
                Data.Instance.cacheData.AddToFilmCache(ScenesManagerFabulab.Instance.currentFDataID, ScenesManagerFabulab.Instance.Scenes);
                LoadSucces();
            }
        }


        public void InitLoadedFilm() {
            if (ScenesManagerFabulab.Instance.Scenes.Count > 0 && ScenesManagerFabulab.Instance.Scenes.Count >= currentFilmData.framecount) {
                ScenesManagerFabulab.Instance.currentSceneId = 1;
                ScenesManagerFabulab.Instance.AddSceneObjectsToScene();
                StoryMakerEvents.OnLoadFilm();
            } else {
                Invoke("InitLoadedFilm", 1);
            }
        }
    }
           
}
