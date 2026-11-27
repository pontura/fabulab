using System;
using System.Collections;
using BoardItems.BoardData;
using MiniGames.Editing;
using Newtonsoft.Json;
using UI;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.StoryMaker.Editor;

namespace WebEditor
{
    // Entry point of the scene Editor (Unity WebGL opened by web/client in /editor). Instead of the home of the
    // app it opens one editor, given by the page URL:
    //   ?type=story|character|object|background        what to edit
    //   &id=<id>                                         edit that work of the user; without id, create a new one
    // The user is the one signed in in the web page (FirebaseRestHostSession). When the app goes back to its home
    // (back button, or after saving) the editor is over and the page is told so.
    // Messages to the page (WebEditorBridge): loading, opened, closed, error
    public class WebEditorController : MonoBehaviour
    {
        public enum WorkType { story, character, @object, background, minigame }

        [Tooltip("Used when the URL has no type (to test in the Unity editor)")]
        [SerializeField] WorkType defaultType = WorkType.character;
        [Tooltip("Used when the URL has no type (to test in the Unity editor). Empty: create a new one")]
        [SerializeField] string defaultId;

        WorkType type;
        string id;
        bool opened;
        bool closed;

        void Awake() {
            Events.ShowScreen += OnShowScreen;
        }

        void Start() {
            string typeParam = GetUrlParameter("type");
            if (string.IsNullOrEmpty(typeParam)) {
                type = defaultType;
                id = defaultId;
            } else {
                if (!Enum.TryParse(typeParam, true, out type)) {
                    Fail("Unknown type: " + typeParam);
                    return;
                }
                id = GetUrlParameter("id");
            }

            if (FirebaseAuthManager.Instance != null)
                FirebaseAuthManager.Instance.OnSignedOut += OnSignedOut;
            WebEditorBridge.Notify("loading", Payload());
        }

        void OnDestroy() {
            Events.ShowScreen -= OnShowScreen;
            if (FirebaseAuthManager.Instance != null)
                FirebaseAuthManager.Instance.OnSignedOut -= OnSignedOut;
        }

        // The app shows its home once the user data is loaded (UIManager.Init), and again when an editor is left
        void OnShowScreen(UIManager.screenType screen) {
            if (screen != UIManager.screenType.Home || closed)
                return;
            // A story or background made from the minigame editor was left: it goes back to the minigame editor
            if (MiniGameCreation.IsActive)
                return;
            if (!opened) {
                opened = true;
                // The other listeners of this event are still showing the home
                StartCoroutine(OpenNextFrame());
            } else {
                closed = true;
                WebEditorBridge.Notify("closed", Payload());
            }
        }

        void OnSignedOut() {
            if (!opened)
                Fail("No signed in user");
        }

        IEnumerator OpenNextFrame() {
            yield return null;
            if (type == WorkType.minigame) {
                OpenMiniGame();
                yield break;
            }
            // The home does not wait for the objects of the users: an object or background to edit needs them
            if (!string.IsNullOrEmpty(id) && (type == WorkType.@object || type == WorkType.background))
                yield return new WaitUntil(() => Data.Instance.sObjectsData.MetadataLoaded);
            // A character of another user is found in the metadata of all the characters
            if (!string.IsNullOrEmpty(id) && type == WorkType.character && Data.Instance.charactersData.GetUserCharacter(id) == null)
                yield return new WaitUntil(() => Data.Instance.charactersData.MetadataLoaded);
            if (string.IsNullOrEmpty(id))
                Create();
            else if (!Edit())
                yield break;
            WebEditorBridge.Notify("opened", Payload());
        }

        // Same entry points as the buttons of the app (with the onboarding videos skipped)
        void Create() {
            switch (type) {
                case WorkType.story: UIManager.Instance.CreateSelected(1, true); break;
                case WorkType.character: UIManager.Instance.CreateSelected(2, true); break;
                case WorkType.@object: UIManager.Instance.CreateSelected(3, true); break;
                case WorkType.background: UIManager.Instance.CreateSelected(4, true); break;
            }
        }

        // Same as opening a work from the user screens of the app (UserStoriesScreen, UserCharactersScreen...).
        // A work of another user ("Modificar" in the web) opens in edition too: saving it makes a new work of the user,
        // that keeps its creators (CharacterEdition / ObjectsEdition SaveNew, StoryEditorScreen.InitSaving)
        bool Edit() {
            switch (type) {
                case WorkType.story:
                    bool ownStory = Data.Instance.scenesData.userFilmsData.Exists(x => x.id == id);
                    if (!ownStory && !Data.Instance.scenesData.filmsData.Exists(x => x.id == id))
                        return Fail("Story not found: " + id);
                    StartCoroutine(EditStory(ownStory));
                    return true;

                case WorkType.character:
                    bool ownCharacter = Data.Instance.charactersData.GetUserCharacter(id) != null;
                    if (!ownCharacter && Data.Instance.charactersData.GetMeta(id) == null)
                        return Fail("Character not found: " + id);
                    Data.Instance.charactersData.Game = false;
                    // One of another user opens ready to edit, not in preview
                    UIManager.Instance.OpenOthersInEdition = !ownCharacter;
                    UIManager.Instance.LoadWork(BoardUI.editingTypes.CHARACTER, id);
                    return true;

                default:
                    // From the metadata, as UserObjectsScreen: the content of the objects of the user (sObjectsData.data)
                    // is loaded later and may not be there yet. LoadWork loads it from the server when it is missing
                    PropMetaData so = Data.Instance.sObjectsData.userMetaData.Find(x => x.id == id)
                        ?? Data.Instance.sObjectsData.GetMeta(id);
                    if (so == null)
                        return Fail("Object not found: " + id);
                    Data.Instance.sObjectsData.SetType(so.type);
                    UIManager.Instance.LoadWork(BoardUI.editingTypes.OBJECT, id);
                    return true;
            }
        }

        // ?type=minigame&id=<id>: edits that minigame (if it is of another user, saving makes a copy). Always from an
        // existing one: the originals of Fabulab are uploaded from the Unity editor (MiniGame: Subir como minijuego oficial)
        void OpenMiniGame() {
            MiniGameEditorScreen editor = FindFirstObjectByType<MiniGameEditorScreen>(FindObjectsInactive.Include);
            if (editor == null) {
                Fail("The scene has no MiniGameEditorScreen");
                return;
            }
            if (string.IsNullOrEmpty(id)) {
                Fail("A minigame is made from an existing one: the id is missing");
                return;
            }
            editor.Open(id, found => {
                if (found)
                    WebEditorBridge.Notify("opened", Payload());
                else
                    Fail("Minigame not found: " + id);
            });
        }

        IEnumerator EditStory(bool own) {
            Events.OnLoading(true);
            UIManager.Instance.boardUI.SetEditingType(BoardUI.editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);
            UIManager.Instance.AddBackTo(UIManager.screenType.StoryMaker, true);
            if (own)
                Data.Instance.scenesData.LoadUserFilm(id);
            else
                Data.Instance.scenesData.LoadFilm(id);
            yield return null;
            yield return null;
            StoryMakerEvents.EnableStoryEdition(true);
        }

        bool Fail(string message) {
            Debug.LogError("[WebEditor] " + message);
            WebEditorBridge.Notify("error", JsonConvert.SerializeObject(new { message, type = type.ToString(), id }));
            return false;
        }

        string Payload() {
            return JsonConvert.SerializeObject(new { type = type.ToString(), id = id ?? "" });
        }

        // Reads a parameter of the page URL (WebGL). Empty outside the browser
        static string GetUrlParameter(string name) {
            string url = Application.absoluteURL;
            if (string.IsNullOrEmpty(url))
                return null;
            int queryStart = url.IndexOf('?');
            if (queryStart < 0)
                return null;
            string query = url.Substring(queryStart + 1);
            int hashStart = query.IndexOf('#');
            if (hashStart >= 0)
                query = query.Substring(0, hashStart);
            foreach (string pair in query.Split('&')) {
                string[] parts = pair.Split(new[] { '=' }, 2);
                if (parts.Length == 2 && Uri.UnescapeDataString(parts[0]) == name)
                    return Uri.UnescapeDataString(parts[1].Replace('+', ' '));
            }
            return null;
        }
    }
}
