using System;
using System.Collections.Generic;
using System.Text;
using BoardItems;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using UnityEngine;
using Yaguar.FirebaseRest;
using Yaguar.StoryMaker.DB;

namespace MiniGames
{
    // Reads and saves the minigames: minigames/{uid}/{id} (MiniGameConfig), metadata/minigames/{id} (MiniGameMetaData)
    // and the thumbnail in images/{uid}/minigames/{id}.jpg. Saving a minigame of another user makes a copy of it in
    // the account of the user; saving one of the user updates it
    public static class MiniGamesData
    {
        public const string Folder = "minigames";

        static string Uid => Data.Instance.userData.userDataInDatabase.uid;

        public static bool IsOwn(MiniGameMetaData meta) {
            return meta != null && !string.IsNullOrEmpty(meta.id) && !string.IsNullOrEmpty(Uid) && meta.userID == Uid;
        }

        // Calls back with null when it does not exist or fails
        public static void LoadMeta(string id, Action<MiniGameMetaData> onDone) {
            Get("metadata/" + Folder + "/" + id, json => {
                MiniGameMetaData meta = Parse<MiniGameMetaData>(json);
                if (meta != null)
                    meta.id = id;
                onDone(meta);
            });
        }

        public static void LoadConfig(MiniGameMetaData meta, Action<MiniGameConfig> onDone) {
            Get(Folder + "/" + meta.userID + "/" + meta.id, json => {
                MiniGameConfig config = Parse<MiniGameConfig>(json);
                if (config != null) {
                    config.Normalize();
                    if (string.IsNullOrEmpty(config.mechanic))
                        config.mechanic = meta.mechanic;
                }
                onDone(config);
            });
        }

        // The metadata and the configuration of a minigame. Calls back with nulls when it does not exist
        public static void Load(string id, Action<MiniGameMetaData, MiniGameConfig> onDone) {
            LoadMeta(id, meta => {
                if (meta == null || string.IsNullOrEmpty(meta.userID)) {
                    onDone(null, null);
                    return;
                }
                LoadConfig(meta, config => onDone(meta, config));
            });
        }

        // The original of Fabulab of a mechanic (the oldest, if there are several). Null when there is none
        public static void FindOfficial(string mechanic, Action<MiniGameMetaData> onDone) {
            LoadAllMeta(all => {
                MiniGameMetaData official = null;
                foreach (MiniGameMetaData m in all) {
                    if (!m.isOfficial || m.mechanic != mechanic) continue;
                    if (official == null || string.CompareOrdinal(m.timestamp, official.timestamp) < 0)
                        official = m;
                }
                onDone(official);
            });
        }

        // Every minigame (public and private: whoever lists them filters)
        public static void LoadAllMeta(Action<List<MiniGameMetaData>> onDone) {
            Get("metadata/" + Folder, json => {
                List<MiniGameMetaData> list = new List<MiniGameMetaData>();
                Dictionary<string, MiniGameMetaData> all = Parse<Dictionary<string, MiniGameMetaData>>(json);
                if (all != null) {
                    foreach (KeyValuePair<string, MiniGameMetaData> e in all) {
                        if (e.Value == null) continue;
                        e.Value.id = e.Key;
                        list.Add(e.Value);
                    }
                }
                onDone(list);
            });
        }

        // Saves the minigame given by meta (null: a new one) with this name and configuration. If meta is of another
        // user, it saves a copy in the account of the user. thumb: the new thumbnail (null: keep the current one).
        // official: a new one is an original of Fabulab (an own one keeps what it was; a copy never is).
        // Calls back with the metadata saved, or null when it fails
        public static void Save(MiniGameMetaData meta, string name, bool isPublic, MiniGameConfig config, Texture2D thumb,
            Action<MiniGameMetaData> onDone, bool official = false) {
            string uid = Uid;
            if (string.IsNullOrEmpty(uid)) {
                Debug.LogError("[MiniGamesData] Saving a minigame without a signed in user");
                onDone(null);
                return;
            }
            bool own = IsOwn(meta);
            string id = own ? meta.id : FirebaseDatabase.DefaultInstance.GetReference(Folder + "/" + uid).Push().Key;
            MiniGameMetaData saved = new MiniGameMetaData {
                id = id,
                userID = uid,
                name = name ?? "",
                mechanic = config.mechanic,
                isPublic = isPublic,
                timestamp = DateTime.UtcNow.ToString("o"),
                // A copy remembers where it comes from; an own minigame keeps its origin
                sourceId = own ? meta.sourceId : meta?.id,
                isOfficial = own ? meta.isOfficial : meta == null && official,
                thumb = thumb != null ? thumb : meta?.thumb
            };

            config.Normalize();
            // The minigame carries copies of every work it shows: the ones it already had are kept, the new ones are
            // copied from the originals (loaded first if needed)
            StoryAssets.LoadMissing(config.CharacterIds(), config.ObjectIds(), config.works, () => {
                config.works = StoryAssets.Build(config.CharacterIds(), config.ObjectIds(), config.works, true);
                Debug.Log("[MiniGamesData] Saving " + id + " with copies of " + StoryAssets.Describe(config.works));
                Write("PUT", Folder + "/" + uid + "/" + id, JsonConvert.SerializeObject(config), ok => {
                    if (!ok) {
                        onDone(null);
                        return;
                    }
                    Dictionary<string, object> metaValues = new Dictionary<string, object> {
                        { "userID", saved.userID },
                        { "name", saved.name },
                        { "mechanic", saved.mechanic ?? "" },
                        { "isPublic", saved.isPublic },
                        { "timestamp", saved.timestamp },
                        { "sourceId", saved.sourceId ?? "" },
                        { "isOfficial", saved.isOfficial },
                    };
                    Write("PATCH", "metadata/" + Folder + "/" + id, JsonConvert.SerializeObject(metaValues), metaOk => {
                        if (!metaOk) {
                            onDone(null);
                            return;
                        }
                        if (saved.thumb == null) {
                            onDone(saved);
                            return;
                        }
                        UploadThumb(saved.thumb, uid, id, () => onDone(saved));
                    });
                });
            });
        }

        // The thumbnail of a minigame
        public static void LoadThumb(MiniGameMetaData meta, Action<Texture2D> onDone) {
            Data.Instance.cacheData.LoadImage(Folder, meta.id, onDone, meta.timestamp, meta.userID);
        }

        static void Get(string path, Action<string> onDone) {
            FirebaseDatabase.DefaultInstance.GetReference(path).GetValueAsync().ContinueWithOnMainThread(task => {
                string json = null;
                if (task.IsFaulted || task.IsCanceled)
                    Debug.LogError("[MiniGamesData] GET " + path + " failed: " + task.Exception);
                else
                    json = task.Result.GetRawJsonValue();
                onDone(json);
            });
        }

        // Writes go with the session of the user. In the Web target the session is the REST one (FirebaseRestSession),
        // also inside the Unity editor, where the database of the SDK has no session and every write is refused:
        // there they are done over REST. Elsewhere, with the SDK
#if UNITY_WEBGL
        static async void Write(string method, string path, string json, Action<bool> onDone) {
            bool ok = false;
            try {
                string token = await FirebaseRestSession.GetIdTokenAsync();
                string url = FirebaseRestConfig.DatabaseUrl + "/" + path + ".json?print=silent&auth=" + Uri.EscapeDataString(token ?? "");
                string httpMethod = method;
                if (method == "PATCH") {
                    // UnityWebRequest in WebGL does not reliably send PATCH; RTDB accepts a method override on a POST
                    httpMethod = "POST";
                    url += "&x-http-method-override=PATCH";
                }
                RestResponse res = await FirebaseRestHttp.SendAsync(httpMethod, url, Encoding.UTF8.GetBytes(json), "application/json");
                ok = res.Ok;
                if (!ok)
                    Debug.LogError("[MiniGamesData] " + method + " " + path + " failed: " + (res.IsNetworkError ? "network error" : res.Status + " " + res.Text));
            } catch (Exception e) {
                Debug.LogError("[MiniGamesData] " + method + " " + path + " failed: " + e.Message);
            }
            onDone(ok);
        }

        static async void UploadThumb(Texture2D thumb, string uid, string id, Action onDone) {
            try {
                string token = await FirebaseRestSession.GetIdTokenAsync();
                string path = "images/" + uid + "/" + Folder + "/" + id + ".jpg";
                string url = "https://firebasestorage.googleapis.com/v0/b/" + FirebaseRestConfig.StorageBucket + "/o?name=" + Uri.EscapeDataString(path);
                Dictionary<string, string> headers = new Dictionary<string, string> { { "Authorization", "Firebase " + token } };
                RestResponse res = await FirebaseRestHttp.SendAsync("POST", url, thumb.EncodeToJPG(), "image/jpeg", headers);
                if (!res.Ok)
                    Debug.LogError("[MiniGamesData] Uploading the thumbnail of " + id + " failed: " + (res.IsNetworkError ? "network error" : res.Status + " " + res.Text));
            } catch (Exception e) {
                Debug.LogError("[MiniGamesData] Uploading the thumbnail of " + id + " failed: " + e.Message);
            }
            onDone();
        }
#else
        static void Write(string method, string path, string json, Action<bool> onDone) {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(path);
            System.Threading.Tasks.Task task = method == "PATCH"
                ? reference.UpdateChildrenAsync(JsonConvert.DeserializeObject<Dictionary<string, object>>(json))
                : reference.SetRawJsonValueAsync(json);
            task.ContinueWithOnMainThread(t => {
                bool ok = !(t.IsFaulted || t.IsCanceled);
                if (!ok)
                    Debug.LogError("[MiniGamesData] " + method + " " + path + " failed: " + t.Exception);
                onDone(ok);
            });
        }

        static void UploadThumb(Texture2D thumb, string uid, string id, Action onDone) {
            _ = FirebaseStoryMakerDBManager.Instance.UploadTexture(thumb, Folder, id, uid, onDone: onDone);
        }
#endif

        static T Parse<T>(string json) where T : class {
            if (string.IsNullOrEmpty(json) || json == "null")
                return null;
            try {
                return JsonConvert.DeserializeObject<T>(json);
            } catch (Exception e) {
                Debug.LogError("[MiniGamesData] Invalid data: " + e.Message);
                return null;
            }
        }
    }
}
