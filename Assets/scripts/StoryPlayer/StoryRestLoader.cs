using BoardItems;
using BoardItems.BoardData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Yaguar.StoryMaker.Editor;

namespace StoryPlayer
{
    // A story ready to be shown: its metadata, its keyframes and the characters and objects it uses
    public class LoadedStory
    {
        public FilmData film;
        public List<SceneData> scenes;
        public List<CharacterData> characters = new List<CharacterData>();
        public List<SObjectData> objects = new List<SObjectData>();
    }

    // Reads a story, and only what it shows, from the Realtime Database REST API.
    // Those paths are public (see database.rules.json), so no session is needed, and it works on every
    // platform, WebGL included (where the Firebase Unity SDK is not available)
    public class StoryRestLoader
    {
        readonly MonoBehaviour host;
        readonly string databaseUrl;

        public StoryRestLoader(MonoBehaviour host, string databaseUrl) {
            this.host = host;
            this.databaseUrl = databaseUrl.TrimEnd('/');
        }

        public IEnumerator LoadStory(string storyId, Action<LoadedStory> onDone, Action<string> onError) {
            LoadedStory story = new LoadedStory();

            // 1. Metadata: says who the author is, needed to find the story
            JToken meta = null;
            yield return Get("metadata/stories/" + storyId, result => meta = result);
            if (meta == null || meta.Type != JTokenType.Object) {
                onError("Story not found: " + storyId);
                yield break;
            }
            try {
                story.film = ParseFilmData(storyId, (JObject)meta);
            } catch (Exception e) {
                onError("Invalid story metadata " + storyId + ": " + e.Message);
                yield break;
            }
            if (string.IsNullOrEmpty(story.film.userID)) {
                onError("Story without author: " + storyId);
                yield break;
            }

            // 2. Keyframes, and the copies of the works the story shows (StoryAssetsData): saved with the story,
            // they are what it shows even if the originals changed or were deleted. Stories saved before the
            // copies existed have none: their works come from the originals
            JToken scenesJson = null;
            JToken assetsJson = null;
            yield return WaitAll(new[] {
                Get("stories/" + story.film.userID + "/" + storyId, result => scenesJson = result),
                Get("storyAssets/" + story.film.userID + "/" + storyId, result => assetsJson = result)
            });
            try {
                story.scenes = ParseScenes(scenesJson);
            } catch (Exception e) {
                Debug.LogException(e);
                onError("Invalid keyframes in story " + storyId + ": " + e.Message);
                yield break;
            }
            if (story.scenes.Count == 0) {
                onError("Story without keyframes: " + storyId);
                yield break;
            }

            JToken copiedCharacters = assetsJson?["characters"];
            JToken copiedObjects = assetsJson?["so"];

            // 3. Characters, objects and backgrounds shown in the keyframes
            HashSet<string> characterIds = new HashSet<string>();
            HashSet<string> objectIds = new HashSet<string>();
            foreach (SceneData scene in story.scenes) {
                foreach (SceneElement element in scene.GetScenesElements()) {
                    if (element.data == null || string.IsNullOrEmpty(element.data.id))
                        continue;
                    if (element.type == SceneElementType.AVATAR)
                        characterIds.Add(element.data.id);
                    else if (element.type == SceneElementType.PROP)
                        objectIds.Add(element.data.id);
                }
                if (!string.IsNullOrEmpty(scene.bgID))
                    objectIds.Add(scene.bgID);
            }

            int copies = 0;
            List<string> fromOriginals = new List<string>();
            foreach (string id in characterIds) {
                if (AddCopiedCharacter(id, copiedCharacters, story))
                    copies++;
                else
                    fromOriginals.Add(id);
            }
            yield return WaitAll(fromOriginals.Select(id => LoadCharacter(id, story)));

            // 4. Objects, then the objects inside them, at any depth (backgrounds, objects on the body parts of
            // characters, objects inside objects)
            HashSet<string> seenObjects = new HashSet<string>(objectIds);
            foreach (string id in InnerObjectIds(story.characters, seenObjects))
                objectIds.Add(id);
            List<string> pending = objectIds.ToList();
            while (pending.Count > 0) {
                fromOriginals.Clear();
                foreach (string id in pending) {
                    if (AddCopiedObject(id, copiedObjects, story))
                        copies++;
                    else
                        fromOriginals.Add(id);
                }
                yield return WaitAll(fromOriginals.Select(id => LoadObject(id, story)));
                pending = InnerObjectIds(story.objects, seenObjects).ToList();
            }

            Debug.Log("[StoryPlayer] Story " + storyId + ": " + copies + " works from its copies, "
                + (story.characters.Count + story.objects.Count - copies) + " from the originals");
            onDone(story);
        }

        // soID of the items not seen yet (it adds them to seen)
        static IEnumerable<string> InnerObjectIds<T>(IEnumerable<T> works, HashSet<string> seen) where T : SOPartData {
            List<string> ids = new List<string>();
            foreach (T work in works) {
                if (work.items == null)
                    continue;
                foreach (SavedIData item in work.items) {
                    if (!string.IsNullOrEmpty(item.soID) && seen.Add(item.soID))
                        ids.Add(item.soID);
                }
            }
            return ids;
        }

        static bool AddCopiedCharacter(string id, JToken copies, LoadedStory story) {
            JToken data = copies?[id]?["data"];
            if (data == null || data.Type != JTokenType.Object)
                return false;
            try {
                CharacterData character = new CharacterData { id = id, version = Str(copies[id]["version"]) };
                character.LoadServerData(data.ToObject<CharacterServerData>(JsonSerializer.CreateDefault()));
                story.characters.Add(character);
                return true;
            } catch (Exception e) {
                Debug.LogError("[StoryPlayer] Invalid copy of character " + id + ", using the original: " + e);
                return false;
            }
        }

        static bool AddCopiedObject(string id, JToken copies, LoadedStory story) {
            JToken data = copies?[id]?["data"];
            if (data == null || data.Type != JTokenType.Object)
                return false;
            try {
                SObjectData obj = new SObjectData { id = id, version = Str(copies[id]["version"]) };
                obj.LoadServerData(data.ToObject<SObjectServerData>(JsonSerializer.CreateDefault()));
                story.objects.Add(obj);
                return true;
            } catch (Exception e) {
                Debug.LogError("[StoryPlayer] Invalid copy of object " + id + ", using the original: " + e);
                return false;
            }
        }

        IEnumerator LoadCharacter(string id, LoadedStory story) {
            string userId = null;
            yield return Get("metadata/characters/" + id + "/userID", result => userId = result?.Type == JTokenType.String ? (string)result : null);
            if (userId == null) {
                Debug.LogWarning("[StoryPlayer] Character without metadata (deleted?): " + id);
                yield break;
            }
            JToken json = null;
            yield return Get("characters/" + userId + "/" + id, result => json = result);
            if (json == null || json.Type != JTokenType.Object) {
                Debug.LogWarning("[StoryPlayer] Character not found: " + id);
                yield break;
            }
            // A broken character is left out: an exception would stop the whole story without telling the page
            try {
                CharacterData character = new CharacterData { id = id };
                character.LoadServerData(json.ToObject<CharacterServerData>(JsonSerializer.CreateDefault()));
                story.characters.Add(character);
            } catch (Exception e) {
                Debug.LogError("[StoryPlayer] Invalid character " + id + ": " + e);
            }
        }

        IEnumerator LoadObject(string id, LoadedStory story) {
            string userId = null;
            yield return Get("metadata/so/" + id + "/userID", result => userId = result?.Type == JTokenType.String ? (string)result : null);
            if (userId == null) {
                Debug.LogWarning("[StoryPlayer] Object without metadata (deleted?): " + id);
                yield break;
            }
            JToken json = null;
            yield return Get("so/" + userId + "/" + id, result => json = result);
            if (json == null || json.Type != JTokenType.Object) {
                Debug.LogWarning("[StoryPlayer] Object not found: " + id);
                yield break;
            }
            try {
                SObjectData obj = new SObjectData { id = id };
                obj.LoadServerData(json.ToObject<SObjectServerData>(JsonSerializer.CreateDefault()));
                story.objects.Add(obj);
            } catch (Exception e) {
                Debug.LogError("[StoryPlayer] Invalid object " + id + ": " + e);
            }
        }

        // Runs the routines at the same time and waits for all of them
        IEnumerator WaitAll(IEnumerable<IEnumerator> routines) {
            List<Coroutine> running = routines.Select(r => host.StartCoroutine(r)).ToList();
            foreach (Coroutine c in running)
                yield return c;
        }

        // GET of a database path. Calls onDone with null when it fails or the path does not exist
        IEnumerator Get(string path, Action<JToken> onDone) {
            string url = databaseUrl + "/" + path + ".json";
            using (UnityWebRequest request = UnityWebRequest.Get(url)) {
                // Without a timeout a stalled request waits forever
                request.timeout = 20;
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success) {
                    Debug.LogError("[StoryPlayer] GET " + url + " failed: " + request.error);
                    onDone(null);
                    yield break;
                }
                JToken token = null;
                try {
                    token = JToken.Parse(request.downloadHandler.text);
                } catch (Exception e) {
                    Debug.LogError("[StoryPlayer] Invalid JSON from " + url + ": " + e.Message);
                }
                onDone(token == null || token.Type == JTokenType.Null ? null : token);
            }
        }

        static FilmData ParseFilmData(string id, JObject meta) {
            FilmData fd = new FilmData();
            fd.id = id;
            fd.framecount = -1;
            fd.name = Str(meta, "name");
            fd.userID = Str(meta, "userID");
            fd.speed = (int)Num(meta, "speed", 0);
            fd.likes = (int)Num(meta, "likes", 0);
            fd.isPublic = Bool(meta, "isPublic", false);
            string timestamp = Str(meta, "timestamp");
            fd.timestamp = string.IsNullOrEmpty(timestamp) ? DateTime.MinValue.ToUniversalTime().ToString("o") : timestamp;
            fd.tags = Items(meta["tags"]).Select(t => Str(t)).Where(t => t != null).ToList();
            return fd;
        }

        // Same parsing as FirebaseStoryMakerDBManager.LoadFilmFromServer. The values are saved as strings
        static List<SceneData> ParseScenes(JToken json) {
            List<SceneData> scenes = new List<SceneData>();
            foreach (JToken child in Items(json)) {
                if (child.Type != JTokenType.Object)
                    continue;
                SceneData sdf = new SceneData();
                sdf.bgID = Str(child["bgID"]);
                sdf.camData = new CamData();
                sdf.transition = Bool(child, "transition", false);
                sdf.duration = Num(child, "duration", ScenesManager.Instance.Keyframe_default_duration);
                sdf.lightingId = Str(child["lightingId"]);
                sdf.lightingValue = (int)Num(child, "lightingValue", 0);
                sdf.camData.pos.x = Num(child, "camData_pos_x", 0);
                sdf.camData.pos.y = Num(child, "camData_pos_y", 0);
                sdf.camData.zoom = (int)Num(child, "camData_zoom", 0);
                sdf.camData.tween = Bool(child, "camData_tween", false);

                List<SceneElement> elements = new List<SceneElement>();
                foreach (JToken se in Items(child["scenesElements"])) {
                    if (se.Type != JTokenType.Object || se["type"] == null)
                        continue;
                    SceneElementType type = (SceneElementType)(int)Num(se, "type", 0);
                    elements.Add(SceneElement.FromJson(se.ToString(Formatting.None), type));
                }
                sdf.CopyScenesElements(elements);
                scenes.Add(sdf);
            }
            return scenes;
        }

        // The database returns lists with keys 0,1,2... as JSON arrays (with nulls in the gaps), and as
        // objects when the keys are sparse. Returns the items in key order either way
        static IEnumerable<JToken> Items(JToken token) {
            if (token is JArray array)
                return array.Where(t => t != null && t.Type != JTokenType.Null);
            if (token is JObject obj)
                return obj.Properties()
                    .OrderBy(p => int.TryParse(p.Name, out int n) ? n : int.MaxValue)
                    .Select(p => p.Value);
            return Enumerable.Empty<JToken>();
        }

        static string Str(JToken token) {
            if (token == null || token.Type == JTokenType.Null)
                return null;
            if (token is JValue value)
                return Convert.ToString(value.Value, CultureInfo.InvariantCulture);
            return token.ToString(Formatting.None);
        }

        static string Str(JToken parent, string key) {
            return Str(parent[key]);
        }

        static float Num(JToken parent, string key, float defaultValue) {
            string s = Str(parent[key]);
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float f) ? f : defaultValue;
        }

        static bool Bool(JToken parent, string key, bool defaultValue) {
            string s = Str(parent[key]);
            return bool.TryParse(s, out bool b) ? b : defaultValue;
        }
    }
}
