using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;
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
        public string timestamp;
    }

    public class ScenesData : MonoBehaviour {

        [field: SerializeField] public Vector2Int ThumbSize { get; private set; }

        string sdSeparator = "|";
        string fieldSeparator = "#";
        public List<FilmDataFabulab> userFilmsData;
        public List<FilmDataFabulab> filmsData;
        public FilmDataFabulab currentFilmData;
        // public string currentName;

        public bool loadedDone;


        string initTimeStamp;

        private void Start() {
            //Events.OnThemesLoadedComplete += LoadThemeFilmMetadataFromServer;
            FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
            StoryMakerEvents.ChangeSpeed += ChangeSpeed;
        }
                
        private void OnDestroy() {
            //Events.OnThemesLoadedComplete -= LoadUserFilmMetadataFromServer;
            FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
            StoryMakerEvents.ChangeSpeed -= ChangeSpeed;
        }
        void ChangeSpeed(int speed) {
            if (currentFilmData != null) {
                currentFilmData.speed = speed;
                ScenesManagerFabulab.Instance.currentFilmData = currentFilmData;
            }
        }

        public void StartNewStory(string storyName) {
            Debug.Log("#StartNewStory");
            ScenesManagerFabulab.Instance.Restart();
            StoryMakerEvents.OnStartNewStory();
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
            ScenesManagerFabulab.Instance.currentFDataID = "";
            if (idExists) {
                Debug.Log("#StartNewStory idExists");
                StartNewStory(storyName);
            }

            loadedDone = true;
        }

        void OnTokenUpdated() {
            if (Data.Instance.userData.IsLogged()) {
                CancelInvoke();
                //LoadUserFilmMetadataFromServer();
                LoadAllFilmMetadataFromServer();                            
        } else
                Invoke("OnTokenUpdated", 1);
        }

        public void LoadAllFilmMetadataFromServer() {
            if (Data.Instance.userData.IsLogged()) {
                ScenesManagerFabulab.Instance.currentFDataID = "";
                userFilmsData = new List<FilmDataFabulab>();
                filmsData = new List<FilmDataFabulab>();
                FirebaseStoryMakerDBManager.Instance.LoadAllFilmDataFromServer(filmsData, OnAddAllFilmsData);
            }
        }

        public void LoadUserFilmMetadataFromServer() {
            if (Data.Instance.userData.IsLogged()) {
                ScenesManagerFabulab.Instance.currentFDataID = "";
                userFilmsData = new List<FilmDataFabulab>();
                FirebaseStoryMakerDBManager.Instance.LoadUserFilmDataFromServer(userFilmsData, OnAddFilmDataFromServer);
            }
            //SortUserFilmsDataByLikes();
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

        public void OnAddAllFilmsData(List<FilmDataFabulab> filmsData, Dictionary<string, ServerFilmData> sfds) {
            OnAddFilmDataFromServer(filmsData, sfds);

            foreach (var film in filmsData) {
                if (film.timestamp == null || film.timestamp == "")
                    film.timestamp = DateTime.MinValue.ToUniversalTime().ToString("o");
            }

            userFilmsData.AddRange(filmsData.FindAll(x => x.userID == Data.Instance.userData.userDataInDatabase.uid));

            Events.OnAllFilmMetadataLoadDone();

            initTimeStamp = DateTime.UtcNow.ToString("o");

            var filmMetadata = FirebaseDatabase.DefaultInstance.GetReference("metadata/stories/");
            filmMetadata.ChildAdded += OnFilmdAdded;
            filmMetadata.ChildChanged += OnFilmChanged;
            filmMetadata.ChildRemoved += OnFilmRemoved;
        }

        public void OnAddFilmDataFromServer(List<FilmDataFabulab> filmsData, Dictionary<string, ServerFilmData> sfds) {
            if (sfds == null) return;

            foreach (KeyValuePair<string, ServerFilmData> e in sfds.OrderByDescending(x => x.Value.timestamp).ToList()) {
                if (filmsData.Find(x => x.id == e.Key) == null) {
                    FilmDataFabulab fd = new FilmDataFabulab();
                    fd.id = e.Key;
                    fd.framecount = -1;
                    fd.likes = e.Value.likes;
                    fd.name = e.Value.name;
                    fd.userID = e.Value.userID;
                    fd.speed = e.Value.speed;
                    if (e.Value.timestamp == null || e.Value.timestamp == "")
                        fd.timestamp = DateTime.MinValue.ToUniversalTime().ToString("o");
                    else
                        fd.timestamp = e.Value.timestamp;
                    fd.thumb = new Texture2D(1, 1);
                    fd.thumb.LoadImage(System.Convert.FromBase64String(e.Value.thumb));
                    filmsData.Add(fd);
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
            
        }

        void OnFilmdAdded(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            

            DataSnapshot snapshot = args.Snapshot;
            if (snapshot.Exists) {

                ServerFilmData sfd = JsonUtility.FromJson<ServerFilmData>(snapshot.GetRawJsonValue());
                if (sfd.timestamp == null || sfd.timestamp == "" || String.Compare(sfd.timestamp,initTimeStamp)<0)
                    return;

                Debug.Log("% OnFilmdAdded: "+sfd.timestamp + " > " + initTimeStamp);
                FilmDataFabulab fd = filmsData.Find(x => x.id == snapshot.Key);
                if (fd != null) {
                    SetFilmChanged(fd, sfd);
                } else {
                    fd = new FilmDataFabulab();
                    fd.id = snapshot.Key;

                    fd.framecount = -1;
                    fd.likes = sfd.likes;
                    fd.name = sfd.name;
                    fd.userID = sfd.userID;
                    fd.speed = sfd.speed;
                    if (sfd.timestamp == null || sfd.timestamp == "")
                        fd.timestamp = DateTime.MinValue.ToUniversalTime().ToString("o");
                    else
                        fd.timestamp = sfd.timestamp;
                    fd.thumb = new Texture2D(1, 1);
                    fd.thumb.LoadImage(System.Convert.FromBase64String(sfd.thumb));
                    filmsData.Insert(0, fd);

                    if (sfd.userID == Data.Instance.userData.userDataInDatabase.uid)
                        userFilmsData.Insert(0, fd);

                    Events.OnFilmMetadataAdded(fd);
                }
            }                
        }

        void OnFilmChanged(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            Debug.Log("% OnFilmChanged: " + args.Snapshot.GetRawJsonValue());

            DataSnapshot snapshot = args.Snapshot;
            if (snapshot.Exists) {
                ServerFilmData sfd = JsonUtility.FromJson<ServerFilmData>(snapshot.GetRawJsonValue());                
                FilmDataFabulab fd = filmsData.Find(x => x.id == snapshot.Key);
                if (fd == null) {
                    return;
                }

                SetFilmChanged(fd, sfd);
                /*if (sfd.userID == Data.Instance.userData.userDataInDatabase.uid)
                    userFilmsData.Insert(0, fd);*/
            }
        }

        void SetFilmChanged(FilmDataFabulab fd, ServerFilmData sfd) {
            fd.name = sfd.name;
            fd.speed = sfd.speed;
            if (sfd.timestamp == null || sfd.timestamp == "")
                fd.timestamp = DateTime.MinValue.ToUniversalTime().ToString("o");
            else
                fd.timestamp = sfd.timestamp;
            fd.thumb = new Texture2D(1, 1);
            fd.thumb.LoadImage(System.Convert.FromBase64String(sfd.thumb));

            filmsData = filmsData.OrderByDescending(x => x.timestamp).ToList();
            userFilmsData = userFilmsData.OrderByDescending(x => x.timestamp).ToList();

            Data.Instance.cacheData.RemoveFilmCache(fd.id);

            Events.OnFilmMetadataUpdated(fd);
        }

        void OnFilmRemoved(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            Debug.Log("% OnFilmRemoved: " + args.Snapshot.GetRawJsonValue());

            DataSnapshot snapshot = args.Snapshot;
            if (snapshot.Exists) {
                FilmDataFabulab fd = filmsData.Find(x => x.id == snapshot.Key);
                if (fd != null) {
                    filmsData.Remove(fd);
                    userFilmsData.Remove(fd);
                    Data.Instance.cacheData.RemoveFilmCache(fd.id);
                    Events.OnFilmMetadataRemoved(fd.id);
                }
            }
            Events.OnLoading(false, LoadingType.Fullscreen);
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
                fd.timestamp = currentFilmData.timestamp;
                userFilmsData.Add(fd);
            } else {
                fd.framecount = ScenesManagerFabulab.Instance.Scenes.Count;
                fd.thumb = currentFilmData.thumb;
                fd.name = currentFilmData.name;
                fd.speed = currentFilmData.speed;
                fd.timestamp = currentFilmData.timestamp;
            }

            userFilmsData = userFilmsData.OrderByDescending(x => x.timestamp).ToList();

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
                sfd.timestamp = fd.timestamp;
                FirebaseStoryMakerDBManager.Instance.SaveFilmDataToServer(fd.id, sfd);

                filmsData = filmsData.OrderByDescending(x => x.timestamp).ToList();
            }
            //Events.OnUpdateFilmIcon();

            AudioManager.Instance.musicManager.Play("work");
            Events.OnPopupTopSignalText("Historia salvada");
        }

        public void LoadFilm(string _id) {
            print("LoadFilm _id:" + _id + " loadedDone: " + loadedDone);
            loadedDone = false;
            currentFilmData = filmsData.Find(x => x.id == _id);
            ScenesManagerFabulab.Instance.currentFilmData = currentFilmData;
            if (currentFilmData != null) {
                ScenesManagerFabulab.Instance.currentFDataID = currentFilmData.id;
                ScenesManagerFabulab.Instance.Scenes = new List<SceneDataFabulab>();
                //Data.Instance.firebaseAuthManager.LoadFilmFromServer(ScenesManager.Instance.currentFDataID, 0);
                if (Data.Instance.cacheData.filmsCache.ContainsKey(ScenesManagerFabulab.Instance.currentFDataID)) {
                    Debug.Log("Load From Cache");
                    ScenesManagerFabulab.Instance.Scenes = Data.Instance.cacheData.GetCacheFilmData(ScenesManagerFabulab.Instance.currentFDataID);
                    LoadSucces();
                } else
                    FirebaseStoryMakerDBManager.Instance.LoadFilmFromServer(currentFilmData, OnFilmLoadedFromServer);
            } else
                Debug.LogError("Cant find film Id "+_id);
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
                    //Data.Instance.firebaseAuthManager.LoadFilmFromServer(ScenesManagerFabulab.Instance.currentFDataID, 0);
                    if (Data.Instance.cacheData.filmsCache.ContainsKey(ScenesManagerFabulab.Instance.currentFDataID)) {
                        Debug.Log("Load From Cache");
                        ScenesManagerFabulab.Instance.Scenes = Data.Instance.cacheData.GetCacheFilmData(ScenesManagerFabulab.Instance.currentFDataID);
                        LoadSucces();
                    } else
                        FirebaseStoryMakerDBManager.Instance.LoadFilmFromServer(currentFilmData, OnFilmLoadedFromServer);
                } else {
                    Debug.Log("User not logged");
                }
            }
        }        

        void LoadSucces() {
            //Debug.Log("____________LoadSucces");
            if(ScenesManagerFabulab.Instance.currentFilmData.userID != Data.Instance.userData.userDataInDatabase.uid) {
                GetAllFilmElements();
                return;
            }
            InitLoadedFilm();
            loadedDone = true;
        }

        void OnFilmLoadedFromServer(bool succes, List<SceneDataFabulab> sds) {
            if (succes) {
                Debug.Log("& OnFilmLoadedFromServer "+ sds.Count);
                //ScenesManagerFabulab.Instance.Scenes = sds.OfType<SceneDataFabulab>().ToList();
                ScenesManagerFabulab.Instance.Scenes = sds;
                Data.Instance.cacheData.AddToFilmCache(ScenesManagerFabulab.Instance.currentFDataID, ScenesManagerFabulab.Instance.Scenes);
                LoadSucces();
            }
        }


        public void InitLoadedFilm() {
            if (ScenesManagerFabulab.Instance.Scenes.Count > 0 && ScenesManagerFabulab.Instance.Scenes.Count >= currentFilmData.framecount) {
                ScenesManagerFabulab.Instance.currentSceneId = 1;
                ScenesManagerFabulab.Instance.AddAllObjectsToScene();
                //Invoke(nameof(StoryMakerEvents.OnLoadFilm),Time.deltaTime);
                StoryMakerEvents.OnLoadFilm();
            } else {
                Invoke("InitLoadedFilm", 1);
            }
        }

        List<(string id, SceneElementType type)> elementsIds;
        void GetAllFilmElements() {
            elementsIds = new List<(string, SceneElementType)>();
            foreach (SceneDataFabulab sdf in ScenesManagerFabulab.Instance.Scenes) {
                elementsIds.AddRange(sdf.GetScenesElements().Where(p => p.type == SceneElementType.AVATAR || p.type == SceneElementType.PROP)
                    .Select(p => (p.data.id, p.type)).Distinct().ToList().Except(elementsIds));
                if (sdf.bgID!="" && !elementsIds.Contains((sdf.bgID, SceneElementType.PROP)))
                    elementsIds.Add((sdf.bgID, SceneElementType.PROP));
            }

            Debug.Log("& Count: "+elementsIds.Count);
            for(int i = elementsIds.Count-1; i > -1 ; i--) {            
                Debug.Log("& " + elementsIds[i].id + " "+ elementsIds[i].type);
                if (elementsIds[i].type == SceneElementType.AVATAR)
                    Data.Instance.charactersData.LoadOthersCharacter(elementsIds[i].id, OnGetAllElements);
                else if (elementsIds[i].type == SceneElementType.PROP)
                    Data.Instance.sObjectsData.LoadOthersObject(elementsIds[i].id, OnGetAllElements);
            }
        }

        void OnGetAllElements(BoardData.SOPartData sOPart) {
            if(sOPart==null)
                return;
            Debug.Log("& ElementsCount: " + elementsIds.Count + " Part: "+ sOPart +" ID: "+sOPart.id);
            SceneElementType type = SceneElementType.AVATAR;
            if (sOPart is BoardData.SObjectData)
                type = SceneElementType.PROP;
            elementsIds.Remove((sOPart.id,type));
            if (elementsIds.Count <= 0) {
                InitLoadedFilm();
                loadedDone = true;
            }
        }


    }
           
}
