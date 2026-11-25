using System;
using System.Collections.Generic;
using System.Linq;
using BoardItems;
using BoardItems.BoardData;
using UnityEngine;

namespace MiniGames.Editing
{
    // What can be chosen for each part of a minigame
    public enum MiniGameOptionKind { Background, Character, Object, Story }

    // A work that can be chosen: of the user, or public of another user
    public class MiniGameOption
    {
        public string id;
        public string name;
        public string userID;
        public string timestamp;
        public bool isOwn;
        // Folder of its thumbnail in Storage (images/{userID}/{folder}/{id}.jpg)
        public string folder;
        public Texture2D thumb;
    }

    // The works the user can choose: own first (newest first), then the public ones of other users (most liked first).
    // The stories and the backgrounds are only the ones with the tag "minigame" (made from the editor, MiniGameCreation)
    public static class MiniGameEditorOptions
    {
        static string Uid => Data.Instance.userData.userDataInDatabase.uid;

        public static List<MiniGameOption> Get(MiniGameOptionKind kind) {
            switch (kind) {
                case MiniGameOptionKind.Character: return Characters();
                case MiniGameOptionKind.Story: return Stories();
                case MiniGameOptionKind.Background: return Objects(SObjectData.types.background, true);
                default: return Objects(SObjectData.types.generic, false);
            }
        }

        // The option of an id, also when it is not in the lists (ex: a private work of the author of the minigame)
        // (or without the tag "minigame": the ones of the minigames made before it)
        public static MiniGameOption Find(MiniGameOptionKind kind, string id) {
            if (string.IsNullOrEmpty(id))
                return null;
            return Get(kind).Find(o => o.id == id) ?? Unlisted(kind, id);
        }

        static MiniGameOption Unlisted(MiniGameOptionKind kind, string id) {
            string folder = Folder(kind);
            if (kind == MiniGameOptionKind.Story) {
                FilmData film = Data.Instance.scenesData.filmsData.Find(f => f.id == id)
                    ?? Data.Instance.scenesData.userFilmsData.Find(f => f.id == id);
                if (film != null) return FromFilm(film, film.userID == Uid, folder);
            } else {
                CharacterMetaData meta = kind == MiniGameOptionKind.Character
                    ? Data.Instance.charactersData.GetMeta(id)
                    : Data.Instance.sObjectsData.GetMeta(id);
                if (meta != null) return ToOptions(new[] { meta }, meta.userID == Uid, folder).First();
            }
            return new MiniGameOption { id = id, name = "", folder = folder, userID = Owner(kind, id) };
        }

        public static void LoadThumb(MiniGameOption option, Action<Texture2D> onDone) {
            if (option.thumb != null) {
                onDone(option.thumb);
                return;
            }
            if (string.IsNullOrEmpty(option.userID)) {
                onDone(null);
                return;
            }
            Data.Instance.cacheData.LoadImage(option.folder, option.id, tex => {
                option.thumb = tex;
                onDone(tex);
            }, option.timestamp, option.userID);
        }

        static string Folder(MiniGameOptionKind kind) {
            switch (kind) {
                case MiniGameOptionKind.Character: return MetadataTypes.characters.ToString();
                case MiniGameOptionKind.Story: return MetadataTypes.stories.ToString();
                default: return MetadataTypes.so.ToString();
            }
        }

        static string Owner(MiniGameOptionKind kind, string id) {
            switch (kind) {
                case MiniGameOptionKind.Character: return Data.Instance.charactersData.GetMeta(id)?.userID;
                case MiniGameOptionKind.Story: return Data.Instance.scenesData.filmsData.Find(f => f.id == id)?.userID;
                default: return Data.Instance.sObjectsData.GetMeta(id)?.userID;
            }
        }

        static List<MiniGameOption> Characters() {
            CharactersData data = Data.Instance.charactersData;
            string folder = Folder(MiniGameOptionKind.Character);
            IEnumerable<CharacterMetaData> own = data.userCharactersMetaData ?? new List<CharacterMetaData>();
            IEnumerable<CharacterMetaData> others = (data.charactersMetaData ?? new List<CharacterMetaData>())
                .Where(m => m.isPublic && m.userID != Uid).OrderByDescending(m => m.likes);
            return ToOptions(own, true, folder).Concat(ToOptions(others, false, folder)).ToList();
        }

        static List<MiniGameOption> Objects(SObjectData.types type, bool onlyTagged) {
            SObjectsData data = Data.Instance.sObjectsData;
            string folder = Folder(MiniGameOptionKind.Object);
            bool Fits(PropMetaData m) => m.type == type && (!onlyTagged || MiniGameCreation.HasTag(m.tags));
            IEnumerable<PropMetaData> own = (data.userMetaData ?? new List<PropMetaData>()).Where(Fits);
            IEnumerable<PropMetaData> others = (data.metaData ?? new List<PropMetaData>())
                .Where(m => Fits(m) && m.isPublic && m.userID != Uid).OrderByDescending(m => m.likes);
            return ToOptions(own, true, folder).Concat(ToOptions(others, false, folder)).ToList();
        }

        static List<MiniGameOption> Stories() {
            ScenesData data = Data.Instance.scenesData;
            string folder = Folder(MiniGameOptionKind.Story);
            IEnumerable<FilmData> own = (data.userFilmsData ?? new List<FilmData>()).Where(f => MiniGameCreation.HasTag(f.tags));
            IEnumerable<FilmData> others = (data.filmsData ?? new List<FilmData>())
                .Where(f => MiniGameCreation.HasTag(f.tags) && f.isPublic && f.userID != Uid).OrderByDescending(f => f.likes);
            return own.Select(f => FromFilm(f, true, folder)).Concat(others.Select(f => FromFilm(f, false, folder))).ToList();
        }

        static IEnumerable<MiniGameOption> ToOptions<T>(IEnumerable<T> metas, bool isOwn, string folder) where T : CharacterMetaData {
            return metas.Select(m => new MiniGameOption {
                id = m.id, name = m.name ?? "", userID = isOwn ? Uid : m.userID, timestamp = m.timestamp,
                isOwn = isOwn, folder = folder, thumb = m.thumb
            });
        }

        static MiniGameOption FromFilm(FilmData f, bool isOwn, string folder) {
            return new MiniGameOption {
                id = f.id, name = f.name ?? "", userID = isOwn ? Uid : f.userID, timestamp = f.timestamp,
                isOwn = isOwn, folder = folder, thumb = f.thumb
            };
        }
    }
}
