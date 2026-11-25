using System;
using System.Collections.Generic;
using System.Linq;
using BoardItems.BoardData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace BoardItems
{
    // Builds the copies of the works something shows (StoryAssetsData): the stories (storyAssets/{uid}/{id}) and the
    // minigames (MiniGameConfig.works) carry them, so they keep showing the same even if the originals change or are
    // deleted. Works are copied with the objects inside them (backgrounds, objects on body parts, objects in objects)
    public static class StoryAssets
    {
        // The works shown by the scenes of a story
        public static void CollectIds(List<SceneData> scenes, HashSet<string> characterIds, HashSet<string> objectIds) {
            foreach (SceneData scene in scenes) {
                foreach (SceneElement element in scene.GetScenesElements()) {
                    string id = element.data?.id;
                    if (string.IsNullOrEmpty(id))
                        continue;
                    if (element.type == SceneElementType.AVATAR)
                        characterIds.Add(id);
                    else if (element.type == SceneElementType.PROP)
                        objectIds.Add(id);
                }
                if (!string.IsNullOrEmpty(scene.bgID))
                    objectIds.Add(scene.bgID);
            }
        }

        // Copies of the works of a story. A work that is not loaded (ex: deleted by its author) keeps the copy the
        // story already had, if any
        public static StoryAssetsData Build(List<SceneData> scenes, StoryAssetsData previous) {
            HashSet<string> characterIds = new HashSet<string>();
            HashSet<string> objectIds = new HashSet<string>();
            CollectIds(scenes, characterIds, objectIds);
            return Build(characterIds, objectIds, previous, false);
        }

        // Copies of these works and of the objects inside them. keepPrevious: a work with a copy in previous keeps
        // it (a copy does not change); otherwise the loaded work is copied and previous is only used for the ones
        // that are not loaded
        public static StoryAssetsData Build(IEnumerable<string> characterIds, IEnumerable<string> objectIds,
            StoryAssetsData previous, bool keepPrevious) {
            StoryAssetsData assets = new StoryAssetsData();
            Queue<string> pendingObjects = new Queue<string>();
            HashSet<string> seenObjects = new HashSet<string>();

            void AddObjectId(string id) {
                if (!string.IsNullOrEmpty(id) && seenObjects.Add(id))
                    pendingObjects.Enqueue(id);
            }
            void AddInnerObjects(SOPartServerData data) {
                if (data?.items == null)
                    return;
                foreach (ServerIData item in data.items)
                    AddObjectId(item?.soID);
            }

            foreach (string id in objectIds)
                AddObjectId(id);

            foreach (string id in characterIds.Where(id => !string.IsNullOrEmpty(id)).Distinct()) {
                StoryCharacterAsset previousAsset = Previous(previous?.characters, id);
                StoryCharacterAsset asset = keepPrevious ? previousAsset ?? CopyCharacter(id) : CopyCharacter(id) ?? previousAsset;
                if (asset == null) {
                    Debug.LogWarning("[StoryAssets] Character not loaded and without a previous copy: " + id);
                    continue;
                }
                assets.characters[id] = asset;
                AddInnerObjects(asset.data);
            }

            while (pendingObjects.Count > 0) {
                string id = pendingObjects.Dequeue();
                StoryObjectAsset previousAsset = Previous(previous?.so, id);
                StoryObjectAsset asset = keepPrevious ? previousAsset ?? CopyObject(id) : CopyObject(id) ?? previousAsset;
                if (asset == null) {
                    Debug.LogWarning("[StoryAssets] Object not loaded and without a previous copy: " + id);
                    continue;
                }
                assets.so[id] = asset;
                AddInnerObjects(asset.data);
            }
            return assets;
        }

        // Loads from the server the works (and the objects inside them) that are neither loaded nor in previous, so
        // Build can copy them. Calls back once everything that could be loaded is
        public static void LoadMissing(IEnumerable<string> characterIds, IEnumerable<string> objectIds,
            StoryAssetsData previous, Action onDone) {
            int pending = 1; // released at the end of this call
            HashSet<string> seen = new HashSet<string>();
            void Done() {
                if (--pending == 0)
                    onDone();
            }
            void Inner(SOPartData data) {
                if (data?.items == null) return;
                foreach (SavedIData item in data.items)
                    LoadObject(item.soID);
            }
            void LoadObject(string id) {
                if (string.IsNullOrEmpty(id) || !seen.Add("so/" + id))
                    return;
                if (previous != null && previous.so.TryGetValue(id, out StoryObjectAsset copy)) {
                    foreach (ServerIData item in copy.data?.items ?? new List<ServerIData>())
                        LoadObject(item?.soID);
                    return;
                }
                SObjectData loaded = FindLoadedObject(id);
                if (loaded != null) {
                    Inner(loaded);
                    return;
                }
                pending++;
                Data.Instance.sObjectsData.LoadOthersObject(id, data => {
                    Inner(data);
                    Done();
                });
            }
            void LoadCharacter(string id) {
                if (string.IsNullOrEmpty(id) || !seen.Add("characters/" + id))
                    return;
                if (previous != null && previous.characters.TryGetValue(id, out StoryCharacterAsset copy)) {
                    foreach (ServerIData item in copy.data?.items ?? new List<ServerIData>())
                        LoadObject(item?.soID);
                    return;
                }
                CharacterData loaded = FindLoadedCharacter(id);
                if (loaded != null) {
                    Inner(loaded);
                    return;
                }
                pending++;
                Data.Instance.charactersData.LoadOthersCharacter(id, data => {
                    Inner(data);
                    Done();
                });
            }

            foreach (string id in characterIds) LoadCharacter(id);
            foreach (string id in objectIds) LoadObject(id);
            Done();
        }

        // The copies as works the app can show, by id (for Items.WorksOverride)
        public static Dictionary<string, SOPartData> ToWorks(StoryAssetsData assets) {
            Dictionary<string, SOPartData> works = new Dictionary<string, SOPartData>();
            if (assets == null)
                return works;
            foreach (KeyValuePair<string, StoryCharacterAsset> e in assets.characters) {
                if (e.Value?.data == null) continue;
                try {
                    CharacterData character = new CharacterData { id = e.Key, version = e.Value.version };
                    character.LoadServerData(Reparse(e.Value.data));
                    works[e.Key] = character;
                } catch (Exception ex) {
                    Debug.LogError("[StoryAssets] Invalid copy of character " + e.Key + ": " + ex.Message);
                }
            }
            foreach (KeyValuePair<string, StoryObjectAsset> e in assets.so) {
                if (e.Value?.data == null) continue;
                try {
                    SObjectData obj = new SObjectData { id = e.Key, version = e.Value.version };
                    obj.LoadServerData(Reparse(e.Value.data));
                    works[e.Key] = obj;
                } catch (Exception ex) {
                    Debug.LogError("[StoryAssets] Invalid copy of object " + e.Key + ": " + ex.Message);
                }
            }
            return works;
        }

        // A copy of the data, so showing it never changes the copy (LoadServerData keeps references to the items)
        static T Reparse<T>(T data) {
            return JToken.FromObject(data).ToObject<T>(JsonSerializer.CreateDefault());
        }

        static T Previous<T>(Dictionary<string, T> previous, string id) where T : class {
            return previous != null && previous.TryGetValue(id, out T asset) ? asset : null;
        }

        static CharacterData FindLoadedCharacter(string id) {
            CharactersData characters = Data.Instance.charactersData;
            return characters.GetUserCharacter(id) ?? characters.othersCharacters?.Find(x => x.id == id);
        }

        static SObjectData FindLoadedObject(string id) {
            SObjectsData objects = Data.Instance.sObjectsData;
            return objects.GetSO(id) ?? objects.othersData?.Find(x => x.id == id);
        }

        static StoryCharacterAsset CopyCharacter(string id) {
            CharactersData characters = Data.Instance.charactersData;
            CharacterData data = FindLoadedCharacter(id);
            if (data?.items == null)
                return null;
            CharacterMetaData meta = characters.GetMeta(id) ?? characters.userCharactersMetaData?.Find(x => x.id == id);
            return new StoryCharacterAsset {
                userID = meta?.userID ?? OwnUserIdIf(characters.IsMyCharacter(id)),
                version = data.version ?? meta?.timestamp,
                data = Reparse(data.GetServerData())
            };
        }

        static StoryObjectAsset CopyObject(string id) {
            SObjectsData objects = Data.Instance.sObjectsData;
            SObjectData data = FindLoadedObject(id);
            if (data?.items == null)
                return null;
            PropMetaData meta = objects.GetMeta(id) ?? objects.userMetaData?.Find(x => x.id == id);
            return new StoryObjectAsset {
                userID = meta?.userID ?? OwnUserIdIf(objects.GetSO(id) != null),
                version = data.version ?? meta?.timestamp,
                data = Reparse(data.GetServerData())
            };
        }

        static string OwnUserIdIf(bool isOwn) {
            return isOwn ? Data.Instance.userData.userDataInDatabase.uid : null;
        }

        // Stats for the log
        public static string Describe(StoryAssetsData assets) {
            return assets == null ? "none" : assets.characters.Count + " characters, " + assets.so.Count + " objects";
        }

        // Ids of the works used by the scenes that have no copy (to warn about them)
        public static IEnumerable<string> Missing(List<SceneData> scenes, StoryAssetsData assets) {
            return scenes.SelectMany(s => s.GetScenesElements())
                .Where(e => e.data != null && !string.IsNullOrEmpty(e.data.id))
                .Where(e => e.type == SceneElementType.AVATAR ? !assets.characters.ContainsKey(e.data.id)
                    : e.type == SceneElementType.PROP && !assets.so.ContainsKey(e.data.id))
                .Select(e => e.data.id)
                .Distinct();
        }
    }
}
