using BoardItems;
using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.DB;
using Yaguar.StoryMaker.Editor;
using static BoardItems.AlbumData;

namespace Yaguar.StoryMaker.DB
{
    public class FirebaseStoryMakerDBManager : FirebaseDBManager
    {
        public new static FirebaseStoryMakerDBManager Instance { get { return mInstance; } }
        static FirebaseStoryMakerDBManager mInstance = null;        

        [Serializable]
        public class PostResponse
        {
            public string name;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                UnityEngine.Debug.LogError($"There should be only one {nameof(FirebaseStoryMakerDBManager)} in the Scene!. Destroying...");
                Destroy(gameObject);
                return;
            }
            mInstance = this;
            DontDestroyOnLoad(this.gameObject);            
            _uid = PlayerPrefs.GetString("uid", "");
        }
        void Start()
        {
            FirebaseAuthManager.Instance.OnFirebaseAuthenticated += OnFirebaseAuthenticated;
        }

        private void OnDestroy()
        {
            FirebaseAuthManager.Instance.OnFirebaseAuthenticated += OnFirebaseAuthenticated;
        }

        void OnFirebaseAuthenticated(string username, string email, string uid) {
            Debug.Log("#OnFirebaseAuthenticated ");
            _uid = uid;
        }

        public override IFirebaseDBManager GetInstance()
        {
            return FirebaseStoryMakerDBManager.Instance;
        }

        public void UpdateWorkToServer(string workId, string workData, System.Action<bool, string> callback) {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("works/" + _uid + "/" + workId);
            string s = JsonConvert.SerializeObject(workData);
            reference.SetRawJsonValueAsync(s).ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#UpdateWorkToServer FAIL");
                    Debug.Log(task.Exception);
                } else {
                    callback(true, workId);
                }
            }, taskScheduler);
            Debug.Log("Server: UpdateWorkToServer");
            //print("UpdateWorkToServer url : " + url);
        }

        public void SaveWorkToServer(string workData, System.Action<bool, string> callback) {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("works/" + _uid);
            string s = JsonConvert.SerializeObject(workData);
            string key = reference.Push().Key;
            reference.Child(key).SetRawJsonValueAsync(s).ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#SaveWorkToServer FAIL");
                    Debug.Log(task.Exception);
                } else {
                    callback(true, key);
                    Debug.Log("Response: " + key);
                }
            }, taskScheduler);
            Debug.Log("Server: SaveWorkToServer");
            //print("SaveWorkToServer url : " + url);
        }

        public void DeleteWork(string workId, System.Action<string> callback) {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("works/" + _uid + "/" + workId);
            reference.RemoveValueAsync().ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#DeleteWork FAIL");
                    Debug.Log(task.Exception);
                } else {
                    //DeleteWorkMetadataFromServer(workId);
                    callback(workId);
                }
            }, taskScheduler);
            Debug.Log("Server: DeleteWork");
            //Debug.Log(url);
        }

        public void LoadUserWorksFromServer(System.Action<Dictionary<string, string>> callback) {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();           

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("works/" + _uid);
            reference.GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#LoadUserWorksFromServer FAIL");
                    Debug.Log(task.Exception);
                } else if (task.IsCompleted) {
                    //SceneDataLyna[] sds = JsonConvert.DeserializeObject<SceneDataLyna[]>(task.Result.GetRawJsonValue());
                    Debug.Log(task.Result.GetRawJsonValue());
                    Dictionary<string, string> d = JsonConvert.DeserializeObject<Dictionary<string, string>>(task.Result.GetRawJsonValue());
                    Debug.Log("# " + d.Count);
                    callback(d);
                }
            }, taskScheduler);
            Debug.Log("Server: LoadUserWorksFromServer");
            //print("LoadWorkFromServer url : " + url);
        }

        public void LoadWorkFromServer(string workId, System.Action<bool, string> callback, string userId=null)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            string uid = _uid;
            if (userId != null)
            {
                uid = userId;
            }

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("works/" + uid + "/" + workId);
            reference.GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#LoadWorkFromServer FAIL");
                    callback(false, null);
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    //SceneDataLyna[] sds = JsonConvert.DeserializeObject<SceneDataLyna[]>(task.Result.GetRawJsonValue());
                    string data = task.Result.GetRawJsonValue();
                    Debug.Log("# "+data);
                    callback(true, data);
                }
            }, taskScheduler);            
            Debug.Log("Server: LoadWorkFromServer");
            //print("LoadWorkFromServer url : " + url);
        }


        public void SaveWorkMetadataToServer(string workId, AlbumData.ServerWorkMetaData swmd)
        {
            Debug.Log("#SaveWorkMetadataToServer");
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("works/mdata/" + workId);
            string s = JsonConvert.SerializeObject(swmd);
            reference.SetRawJsonValueAsync(s);
            Debug.Log("Server: SaveWorkMetadataToServer");           
        }

        public void LoadUserWorkMetadataFromServer(System.Action<Dictionary<string, AlbumData.ServerWorkMetaData>> callback)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("works/mdata");
            reference.OrderByChild("userID").EqualTo(_uid).GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#LoadUserWorkMetadataFromServer FAIL");                    
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Dictionary<string, AlbumData.ServerWorkMetaData> d = JsonConvert.DeserializeObject<Dictionary<string, AlbumData.ServerWorkMetaData>>(task.Result.GetRawJsonValue());
                    callback(d);
                    // Do something with snapshot...
                }
            }, taskScheduler);
            
            Debug.Log("Server: LoadUserWorkMetadataFromServer");
            //Debug.Log(url);
        }

        /*
        public void DeleteWorkMetadataFromServer(string themeId, string filmId)
        {
            //Debug.Log("ACA");
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("stories/mdata/" + themeId + "/" + filmId);
            reference.RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#DeleteWorkMetadataFromServer FAIL");
                    Debug.Log(task.Exception);
                }
            });
            Debug.Log("Server: DeleteWorkMetadataFromServer");
            //Debug.Log(url);
        }        */

        /*
        public void SaveFilmToServer(List<SceneData> sd, System.Action<bool, string> callback)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("stories/" + _uid );
            string s = JsonConvert.SerializeObject(sd);
            string key = reference.Push().Key;
            reference.Child(key).SetRawJsonValueAsync(s).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#SaveFilmToServer FAIL");
                    Debug.Log(task.Exception);
                }
                else
                {
                    callback(true, key);
                    Debug.Log("Response: " + key);
                }
            }, taskScheduler);
            Debug.Log("Server: SaveFilmToServer");
            //print("SaveFilmToServer url : " + url);
        }

        public void DeleteFilm(ScenesData.FilmDataLyna fd, System.Action<string> callback)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("stories/mdata/" + _uid + "/" + fd.id);
            reference.RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#DeleteFilm FAIL");
                    Debug.Log(task.Exception);
                }
                else
                {
                    DeleteFilmDataFromTheme(fd.themeId, fd.id);
                    callback(fd.id);
                }
            }, taskScheduler);            
            Debug.Log("Server: DeleteFilmDataFromTheme");
            //Debug.Log(url);
        }

        /*public void LoadFilmFromServer(ScenesData.FilmDataLyna fd, System.Action<bool, SceneDataLyna[]> callback)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            string uid = _uid;
            if (fd.userID != null)
            {
                uid = fd.userID;
            }

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("stories/" + uid + "/" + fd.id);
            reference.GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#LoadFilmFromServer FAIL");
                    callback(false, null);
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    SceneDataLyna[] sds = JsonConvert.DeserializeObject<SceneDataLyna[]>(task.Result.GetRawJsonValue());
                    Debug.Log("# "+sds.Length);
                    callback(true, sds);
                    // Do something with snapshot...
                }
            }, taskScheduler);            
            Debug.Log("Server: LoadFilmFromServer");
            //print("LoadFilmFromServer url : " + url);
        }*/

        public void InventoryItemToServer(string type, string data)
        {
            Dictionary<string, System.Object> childUpdates = new Dictionary<string, System.Object>();
            childUpdates[data] = "";

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("inventory/" + _uid + "/" + type);
            reference.UpdateChildrenAsync(childUpdates).ContinueWith( task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#InventoryItemToServer FAIL");
                    Debug.Log(task.Exception);
                }
            });
            Debug.Log("Server: InventoryItemToServer");
            //print("SaveInventoryItem url : " + url);
        }

        /*public void LoadInventoryItemFromServer(CataloguesData.ItemData.types type, System.Action<List<string>, CataloguesData.ItemData.types, bool> callback)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("inventory/" + _uid + "/" + type.ToString());
            reference.GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#LoadInventoryItemFromServer FAIL");
                    callback(null, type, false);
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Dictionary<string, string> d = JsonConvert.DeserializeObject<Dictionary<string, string>>(task.Result.GetRawJsonValue());
                    callback(d.Keys.ToList<string>(), type, true);
                    // Do something with snapshot...
                }
            }, taskScheduler);
            Debug.Log("Server: LoadInventoryItemFromServer " + type.ToString());
            //Debug.Log(url);
        }

        public void LoadCataloguesFromServer(System.Action<Dictionary<string, CataloguesData.Categories>, bool> callback)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("catalogs");
            reference.GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#LoadCataloguesFromServer FAIL");
                    callback(null, false);
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Dictionary<string, CataloguesData.Categories> d = JsonConvert.DeserializeObject<Dictionary<string, CataloguesData.Categories>>(task.Result.GetRawJsonValue());
                    callback(d, true);
                }
            }, taskScheduler);
            Debug.Log("Server: LoadCataloguesFromServer");
            //Debug.Log(url);
        }*/
        
        public void GetUsernameFromServer(string uid, System.Action<string, string> callback)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("users/" + uid);
            reference.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#GetUsernameFromServer FAIL");
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    callback(uid, task.Result.GetRawJsonValue());
                }
            }, taskScheduler);
            Debug.Log("Server: GetUsernameFromServer " + uid);
        }
    }
}
