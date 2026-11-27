using System;
using System.Collections.Generic;

namespace MiniGames.Editing
{
    // A work being created (story or background: the "Crear" buttons) or edited (also characters and objects: the
    // "Editar" buttons) from the minigame editor. While it lasts:
    // - the work saved gets the tag "minigame" (ScenesData, SObjectsData, CharactersData); the minigame editor offers
    //   only the stories and backgrounds with it
    // - leaving its editor (to the home: after saving, or with the back button) goes back to the minigame editor,
    //   that assigns it if it was saved (MiniGameEditorScreen). WebEditorController does not close the web editor
    public static class MiniGameCreation
    {
        public const string TagName = "minigame";

        static MiniGameOptionKind? kind;

        // What is being created. null: nothing
        public static MiniGameOptionKind? Kind => kind;
        public static bool IsActive => kind != null;
        // The id of the work, once saved
        public static string CreatedId { get; private set; }
        // The id of the tag "minigame" (TagsManager), given by Begin
        public static string TagId { get; private set; }

        public static bool IsCreating(MiniGameOptionKind what) => kind == what;

        // The id of the tag (created if there is none yet). onDone(null): it could not be had
        public static void GetTagId(Action<string> onDone) {
            TagsManager tags = Data.Instance.tagsManager;
            string id = tags.GetTagID(TagName);
            if (!string.IsNullOrEmpty(id)) {
                onDone(id);
                return;
            }
            tags.AddTag(TagName, (ok, tag) => onDone(ok && tag != null ? tag.id : null));
        }

        internal static void Begin(MiniGameOptionKind what, string tagId) {
            kind = what;
            TagId = tagId;
            CreatedId = null;
        }

        internal static void End() {
            kind = null;
            CreatedId = null;
        }

        // Called when the work is saved
        public static void OnCreated(string id) {
            if (IsActive && !string.IsNullOrEmpty(id))
                CreatedId = id;
        }

        // Whether a work with these tags is one made for the minigames
        public static bool HasTag(List<string> tags) {
            if (tags == null) return false;
            string id = TagId ?? Data.Instance.tagsManager.GetTagID(TagName);
            return !string.IsNullOrEmpty(id) && tags.Contains(id);
        }
    }
}
