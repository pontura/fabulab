using BoardItems;
//using static BoardItems.AlbumData;
using BoardItems.BoardData;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.DB;
using Yaguar.StoryMaker.Editor;

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

#if UNITY_EDITOR
            var db = FirebaseDatabase.DefaultInstance;
            db.SetPersistenceEnabled(false);
#endif
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

        public void UpdateCharacterToServer(string characterId, CharacterServerData characterData, System.Action<bool, string> callback) {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("characters/" + _uid + "/" + characterId);
            string s = JsonConvert.SerializeObject(characterData);
            reference.SetRawJsonValueAsync(s).ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#UpdateCharacterToServer FAIL");
                    Debug.Log(task.Exception);
                } else {
                    try {
                        callback(true, characterId);
                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }
                }
            });
            Debug.Log("Server: UpdateCharacterToServer");
            //print("UpdateCharacterToServer url : " + url);
        }

        public void SaveCharacterToServer(CharacterServerData characterData, System.Action<bool, string> callback) {
            if (_uid == null || _uid.Length == 0) {
                Debug.LogError("Trying to save character without user id");
                return;
            }
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("characters/" + _uid);
            string s = JsonConvert.SerializeObject(characterData);
            string key = reference.Push().Key;
            reference.Child(key).SetRawJsonValueAsync(s).ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#SaveCharacterToServer FAIL");
                    Debug.Log(task.Exception);
                } else {
                    try { 
                    callback(true, key);
                    Debug.Log("Response: " + key);
                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }
                }
            });
            Debug.Log("Server: SaveCharacterToServer");
            //print("SaveCharacterToServer url : " + url);
        }

        public void DeleteCharacter(string presetId, System.Action<string> callback) {

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("characters/" + _uid + "/" + presetId);
            reference.RemoveValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#DeleteCharacter FAIL");
                    Debug.Log(task.Exception);
                } else {
                    //DeleteCharacterMetadataFromServer(characterId);
                    try { 
                        callback(presetId);
                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }
                }
            });
            Debug.Log("Server: DeleteCharacter");
            //Debug.Log(url);
        }

        public void LoadUserCharactersFromServer(System.Action<Dictionary<string, CharacterServerData>> callback) {

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("characters/" + _uid);
            reference.GetValueAsync().ContinueWithOnMainThread(task => {

                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#LoadUserCharactersFromServer FAIL");
                    Debug.Log(task.Exception);
                } else if (task.IsCompleted) {
                    try {
                        //SceneDataLyna[] sds = JsonConvert.DeserializeObject<SceneDataLyna[]>(task.Result.GetRawJsonValue());
                        //Debug.Log(task.Result.GetRawJsonValue());
                        DataSnapshot snapshot = task.Result;
                        Dictionary<string, CharacterServerData> d = new Dictionary<string, CharacterServerData>();
                        foreach (var child in snapshot.Children) {
                            string json = JsonConvert.SerializeObject(child.Value);
                            Debug.Log(json);
                            if(!json.Contains("#"))
                                d.Add(child.Key, JsonConvert.DeserializeObject<CharacterServerData>(json));
                        }
                        /*Dictionary<string, string> d = JsonConvert.DeserializeObject<Dictionary<string, string>>(task.Result.GetRawJsonValue());
                        Debug.Log("# " + d.Count);*/
                        callback(d);
                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }

                }
            });
            Debug.Log("Server: LoadUserCharactersFromServer");
            //print("LoadCharacterFromServer url : " + url);
        }

        public void LoadCharacterFromServer(string characterId, System.Action<bool, string> callback, string userId=null)
        {
            string uid = _uid;
            if (userId != null)
            {
                uid = userId;
            }

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("characters/" + uid + "/" + characterId);
            reference.GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#LoadCharacterFromServer FAIL");
                    callback(false, null);
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    try { 
                    //SceneDataLyna[] sds = JsonConvert.DeserializeObject<SceneDataLyna[]>(task.Result.GetRawJsonValue());
                    string data = task.Result.GetRawJsonValue();
                    Debug.Log("# "+data);
                    callback(true, data);
                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }
                }
            });            
            Debug.Log("Server: LoadCharacterFromServer");
            //print("LoadCharacterFromServer url : " + url);
        }


        public void SaveCharacterMetadataToServer(string characterId, ServerCharacterMetaData swmd)
        {
            Debug.Log("#SaveCharacterMetadataToServer");
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("metadata/characters/" + characterId);
            string s = JsonConvert.SerializeObject(swmd);
            reference.SetRawJsonValueAsync(s);
            Debug.Log("Server: SaveCharacterMetadataToServer "+ characterId);           
        }

        public void LoadUserCharacterMetadataFromServer(System.Action<List<CharacterMetaData>> callback) {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("metadata/characters");
            Debug.Log("#_uid: " + _uid);
            reference.OrderByChild("userID").EqualTo(_uid).GetValueAsync().ContinueWithOnMainThread(task => {
            //reference.OrderByChild("user_id").EqualTo(_uid).GetValueAsync().ContinueWithOnMainThread(task => {

                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#LoadUserCharacterMetadataFromServer FAIL");                    
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("#LoadUserCharacterMetadataFromServer IsCompleted");
                    try {
                        Debug.Log(task.Result.GetRawJsonValue());
                        DataSnapshot snapshot = task.Result;
                    
                        if (snapshot.Exists) {
                            Debug.Log("#LoadUserCharacterMetadataFromServer exists");
                            List<CharacterMetaData> charactersMetaData = new List<CharacterMetaData>();
                            Debug.Log("#LoadUserCharacterMetadataFromServer snapshot Count: " + snapshot.Children.Count<DataSnapshot>());

                            foreach (var child in snapshot.Children) {
                                Debug.Log(child.Key + ": " + child.Value.ToString());
                                CharacterMetaData fd = new CharacterMetaData();
                                fd.id = child.Key;
                                fd.userID = child.Child("userID").Value as string;
                                fd.thumb = new Texture2D(1, 1);
                                fd.thumb.LoadImage(System.Convert.FromBase64String(child.Child("thumb").Value as string));
                                charactersMetaData.Add(fd);
                            }
                            //Dictionary<string, ServerCharacterMetaData> d = JsonConvert.DeserializeObject<Dictionary<string, ServerCharacterMetaData>>(task.Result.GetRawJsonValue());
                            callback(charactersMetaData);
                        } else {
                            Debug.LogWarning("No se encontraron datos para ese userID");
                        }
                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }
                }
            });
            
            Debug.Log("Server: LoadUserCharacterMetadataFromServer _uid: " + _uid);
            //Debug.Log(url);
        }

        public void LoadCharactersMetadataFromServer(System.Action<List<CharacterMetaData>> callback) {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("metadata/characters");
            reference.GetValueAsync().ContinueWithOnMainThread(task => {

                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#LoadCharactersMetadataFromServer FAIL");
                    Debug.Log(task.Exception);
                } else if (task.IsCompleted) {
                    Debug.Log("#LoadCharactersMetadataFromServer IsCompleted");
                    try {
                        Debug.Log(task.Result.GetRawJsonValue());
                        DataSnapshot snapshot = task.Result;

                        if (snapshot.Exists) {
                            Debug.Log("#LoadCharactersMetadataFromServer exists");
                            List<CharacterMetaData> charactersMetaData = new List<CharacterMetaData>();
                            Debug.Log("#LoadCharactersMetadataFromServer snapshot Count: " + snapshot.Children.Count<DataSnapshot>());

                            foreach (var child in snapshot.Children) {
                                Debug.Log(child.Key + ": " + child.Value.ToString());
                                CharacterMetaData fd = new CharacterMetaData();
                                fd.id = child.Key;
                                fd.userID = child.Child("userID").Value as string;
                                fd.thumb = new Texture2D(1, 1);
                                fd.thumb.LoadImage(System.Convert.FromBase64String(child.Child("thumb").Value as string));
                                charactersMetaData.Add(fd);
                            }
                            //Dictionary<string, ServerCharacterMetaData> d = JsonConvert.DeserializeObject<Dictionary<string, ServerCharacterMetaData>>(task.Result.GetRawJsonValue());
                            callback(charactersMetaData);
                        } else {
                            Debug.LogWarning("snapshot not exist");
                        }
                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }
                }
            });

            Debug.Log("Server: LoadCharactersMetadataFromServer");
            //Debug.Log(url);
        }

        public void LoadBodypartPresetsFromServer(int partId, System.Action<int, Dictionary<string, CharacterPartServerData>> callback) {

            string pId = BoardItems.Characters.CharacterPartsHelper.GetServerUniquePartsId(partId);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("presets/bodypart/" + pId);
            reference.GetValueAsync().ContinueWithOnMainThread(task => {

                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#LoadBodypartPresetsFromServer FAIL");
                    Debug.Log(task.Exception);
                } else if (task.IsCompleted) {
                    //SceneDataLyna[] sds = JsonConvert.DeserializeObject<SceneDataLyna[]>(task.Result.GetRawJsonValue());
                    //Debug.Log(task.Result.GetRawJsonValue());

                    try {
                                             

                        DataSnapshot snapshot = task.Result;
                        Dictionary<string, CharacterPartServerData> d = new Dictionary<string, CharacterPartServerData>();
                        foreach (var child in snapshot.Children) {
                                string json = JsonConvert.SerializeObject(child.Value);
                                //Debug.Log(json);
                                if (!json.Contains("#"))
                                    d.Add(child.Key, JsonConvert.DeserializeObject<CharacterPartServerData>(json));
                        }
                        //string data = task.Result.GetRawJsonValue();
                        //Debug.Log(data);
                        if (d.Count>0) {
                            //Dictionary<string, string> d = JsonConvert.DeserializeObject<Dictionary<string, string>>(task.Result.GetRawJsonValue());
                            //Debug.Log("# " + d.Count);
                            callback(partId, d);
                        } else
                            callback(partId, null);

                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }
                }
            });
            Debug.Log("Server: LoadBodypartPresetsFromServer " + pId);
            //print("LoadCharacterFromServer url : " + url);
        }

        public void UpdateBodypartPresetToServer(string presetId, CharacterPartServerData presetData, string partId, System.Action<bool, string, string> callback) {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("presets/bodypart/" + partId + "/"  + presetId);
            string s = JsonConvert.SerializeObject(presetData);
            reference.SetRawJsonValueAsync(s).ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#UpdateBodypartPresetToServer FAIL");
                    Debug.Log(task.Exception);
                } else {
                    try { 
                        callback(true, presetId, partId);
                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }
                }
            });
            Debug.Log("Server: UpdateBodypartPresetToServer");
            //print("UpdateCharacterToServer url : " + url);
        }

        public void SaveBodypartPresetToServer(CharacterPartServerData presetData, string partId, System.Action<bool, string, string> callback) {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("presets/bodypart/" + partId );
            string s = JsonConvert.SerializeObject(presetData);
            string key = reference.Push().Key;
            reference.Child(key).SetRawJsonValueAsync(s).ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#SaveBodypartPresetToServer FAIL");
                    Debug.Log(task.Exception);
                } else {
                    try {
                        callback(true, key, partId);
                        Debug.Log("Response: " + key);
                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }
                }
            });
            Debug.Log("Server: SaveBodypartPresetToServer");
            //print("SaveCharacterToServer url : " + url);
        }

        public void DeleteBodypartPreset(string presetId, string partId, System.Action<string> callback) {

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("presets/bodypart/" + partId + "/" + presetId);
            Debug.Log("#DeleteBodypartPreset presets / bodypart / " + partId + " / " + presetId);
            reference.RemoveValueAsync().ContinueWithOnMainThread((Action<Task>)(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#DeleteBodypartPreset FAIL");
                    Debug.Log(task.Exception);
                } else {
                    try { 
                    DeleteBodypartPresetMetadataFromServer(presetId);
                    callback((string)presetId);
                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }
                }
            }));
            Debug.Log("Server: DeleteBodypartPreset");
            //Debug.Log(url);
        }

        public void SaveBodypartPresetMetadataToServer(string presetId, ServerPartMetaData swmd) {
            Debug.Log("#SaveBodypartPresetMetadataToServer");
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("metadata/presets/bodypart/" + presetId);
            string s = JsonConvert.SerializeObject(swmd);
            reference.SetRawJsonValueAsync(s);
            Debug.Log("Server: SaveBodypartPresetMetadataToServer");
        }

        public void LoadBodypartPresetMetadataFromServer(System.Action<Dictionary<string, ServerPartMetaData>> callback) {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("metadata/presets/bodypart");
            reference.GetValueAsync().ContinueWithOnMainThread(task => {

                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#LoadBodypartPresetMetadataFromServer FAIL");
                    Debug.Log(task.Exception);
                } else if (task.IsCompleted) {
                    try { 
                    DataSnapshot snapshot = task.Result;
                    Dictionary<string, ServerPartMetaData> d = new Dictionary<string, ServerPartMetaData>();
                    foreach (var child in snapshot.Children) {
                        //Debug.Log(child.Key + " => " + child.Child("name").Value);
                        ServerPartMetaData spmd = new ServerPartMetaData();
                        spmd.partID = child.Child("partID").Value as string;
                        spmd.thumb = child.Child("thumb").Value as string;
                        d.Add(child.Key, spmd);
                    }

                    /*string result = task.Result.GetRawJsonValue();
                    Debug.Log(result);
                    Dictionary<string, ServerPartMetaData> d = JsonConvert.DeserializeObject<Dictionary<string, ServerPartMetaData>>(result);
                    Debug.Log(d == null);*/
                    callback(d);
                        // Do something with snapshot...
                    } catch (Exception ex) {
                        Debug.LogError($"Error en callback: {ex}");
                    }
                }
            });

            Debug.Log("Server: LoadBodypartPresetMetadataFromServer");
            //Debug.Log(url);
        }

        
        public void DeleteBodypartPresetMetadataFromServer(string presetId)
        {
            //Debug.Log("ACA");
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("metadata/presets/bodypart/" + presetId);
            reference.RemoveValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#DeleteBodypartPresetMetadataFromServer FAIL");
                    Debug.Log(task.Exception);
                }
            });
            Debug.Log("Server: DeleteBodypartPresetMetadataFromServer");
            //Debug.Log(url);
        }

        public void SaveFilmDataToServer(string filmId, ScenesData.ServerFilmData fd) {
            Debug.Log("#SaveFilmDataToServer");
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("metadata/stories/" + filmId);
            string s = JsonConvert.SerializeObject(fd);
            reference.SetRawJsonValueAsync(s);
            Debug.Log("Server: SaveFilmDataToServer");
        }

        public void LoadUserFilmDataFromServer(System.Action<Dictionary<string, ScenesData.ServerFilmData>> callback) {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("metadata/stories/");
            reference.OrderByChild("userID").EqualTo(_uid).GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#LoadUserFilmDataFromServer FAIL");
                    if (Data.Instance.scenesData.userFilmsData.Count > 0)
                        Data.Instance.scenesData.OnFilmDataFromServerCompleted();
                    Debug.Log(task.Exception);
                } else if (task.IsCompleted) {
                    Dictionary<string, ScenesData.ServerFilmData> d = JsonConvert.DeserializeObject<Dictionary<string, ScenesData.ServerFilmData>>(task.Result.GetRawJsonValue());
                    callback(d);
                    // Do something with snapshot...
                }
            }, taskScheduler);

            Debug.Log("Server: LoadUserFilmDataFromServer ");
            //Debug.Log(url);
        }

        public void DeleteFilmData(string filmId) {
            //Debug.Log("ACA");
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("metadata/stories/" + filmId);
            reference.RemoveValueAsync().ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#DeleteFilmData FAIL");
                    Debug.Log(task.Exception);
                }
            });
            Debug.Log("Server: DeleteFilmData");
            //Debug.Log(url);
        }

        public void UpdateFilmToServer(string filmId, List<SceneData> sd, System.Action<bool, string> callback) {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("stories/" + _uid + "/" + filmId);
            string s = JsonConvert.SerializeObject(sd);
            reference.SetRawJsonValueAsync(s).ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#UpdateFilmToServer FAIL");
                    Debug.Log(task.Exception);
                } else {
                    callback(true, filmId);
                }
            }, taskScheduler);
            Debug.Log("Server: UpdateFilmToServer");
            //print("UpdateFilmToServer url : " + url);
        }

        public void SaveFilmToServer(List<SceneData> sd, System.Action<bool, string> callback) {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("stories/" + _uid);
            string s = JsonConvert.SerializeObject(sd);
            string key = reference.Push().Key;
            reference.Child(key).SetRawJsonValueAsync(s).ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#SaveFilmToServer FAIL");
                    Debug.Log(task.Exception);
                } else {
                    callback(true, key);
                    Debug.Log("Response: " + key);
                }
            }, taskScheduler);
            Debug.Log("Server: SaveFilmToServer");
            //print("SaveFilmToServer url : " + url);
        }

        public void DeleteFilm(ScenesData.FilmDataFabulab fd, System.Action<string> callback) {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("stories/" + _uid + "/" + fd.id);
            reference.RemoveValueAsync().ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#DeleteFilm FAIL");
                    Debug.Log(task.Exception);
                } else {
                    DeleteFilmData(fd.id);
                    callback(fd.id);
                }
            }, taskScheduler);
            Debug.Log("Server: DeleteFilm");
            //Debug.Log(url);
        }

        public void LoadFilmFromServer(ScenesData.FilmDataFabulab fd, System.Action<bool, SceneData[]> callback) {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            string uid = _uid;
            if (fd.userID != null) {
                uid = fd.userID;
            }

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("stories/" + uid + "/" + fd.id);
            reference.GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#LoadFilmFromServer FAIL");
                    callback(false, null);
                    Debug.Log(task.Exception);
                } else if (task.IsCompleted) {
                    SceneData[] sds = JsonConvert.DeserializeObject<SceneData[]>(task.Result.GetRawJsonValue());
                    Debug.Log("# " + sds.Length);
                    callback(true, sds);
                    // Do something with snapshot...
                }
            }, taskScheduler);
            Debug.Log("Server: LoadFilmFromServer");
            //print("LoadFilmFromServer url : " + url);
        }


        public void InventoryItemToServer(string type, string data)
        {
            Dictionary<string, System.Object> childUpdates = new Dictionary<string, System.Object>();
            childUpdates[data] = "";

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("inventory/" + _uid + "/" + type);
            reference.UpdateChildrenAsync(childUpdates).ContinueWithOnMainThread( task =>
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
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("users/" + uid);
            reference.GetValueAsync().ContinueWithOnMainThread(task =>
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
            });
            Debug.Log("Server: GetUsernameFromServer " + uid);
        }
    }
}
