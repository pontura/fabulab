using System;
using System.Collections;
using MiniGames;
using Newtonsoft.Json;
using UI;
using UnityEngine;
using Yaguar.Auth;

namespace WebMiniGames
{
    // Entry point of the scene MiniGames (Unity WebGL opened by web/client in /minigames). Instead of the home of
    // the app it opens the minigame screen (MiniGameScreen), with the characters and stories of the user signed in
    // in the web page (FirebaseRestHostSession). When the app goes back to its home (back button) the minigames
    // are over and the page is told so.
    // Messages to the page (WebMiniGamesBridge): loading, opened, closed, error
    public class WebMiniGamesController : MonoBehaviour
    {
        bool opened;
        bool closed;

        void Awake() {
            Events.ShowScreen += OnShowScreen;
        }

        void Start() {
            if (FirebaseAuthManager.Instance != null)
                FirebaseAuthManager.Instance.OnSignedOut += OnSignedOut;
            Step("Cargando tus personajes e historias…");
        }

        // Tells the page what the loading is doing now (shown under its spinner)
        static void Step(string message) {
            WebMiniGamesBridge.Notify("loading", JsonConvert.SerializeObject(new { message }));
        }

        void OnDestroy() {
            Events.ShowScreen -= OnShowScreen;
            if (FirebaseAuthManager.Instance != null)
                FirebaseAuthManager.Instance.OnSignedOut -= OnSignedOut;
        }

        // The app shows its home once the user data is loaded (UIManager.Init), and again when the minigames are left
        void OnShowScreen(UIManager.screenType screen) {
            if (screen != UIManager.screenType.Home || closed)
                return;
            if (!opened) {
                opened = true;
                // The other listeners of this event are still showing the home
                StartCoroutine(OpenNextFrame());
            } else {
                closed = true;
                WebMiniGamesBridge.Notify("closed");
            }
        }

        void OnSignedOut() {
            if (!opened)
                Fail("No signed in user");
        }

        // The game asked by the page (?game=<id>): a minigame saved by a user (MiniGamesData), or the id of a mechanic
        // (MiniGame.Id, ex: runner) to play its original of Fabulab. Without it, the one of the game of the screen
        IEnumerator OpenNextFrame() {
            yield return null;
            MiniGameScreen screen = FindFirstObjectByType<MiniGameScreen>(FindObjectsInactive.Include);
            MiniGame game = screen != null ? screen.Game : null;
            if (game == null) {
                Fail("The scene has no minigame");
                yield break;
            }
            string asked = GetUrlParameter("game");
            Step("Cargando el juego…");
            if (string.IsNullOrEmpty(asked) || asked == game.Id) {
                // The id of the mechanic: its original of Fabulab. Until there is one, what the inspector sets
                MiniGamesData.FindOfficial(game.Id, official => {
                    if (official == null) {
                        Play(game.Id);
                        return;
                    }
                    LoadAndPlay(game, official.id);
                });
                yield break;
            }
            LoadAndPlay(game, asked);
        }

        void LoadAndPlay(MiniGame game, string asked) {
            MiniGamesData.Load(asked, (meta, config) => {
                if (meta == null || config == null) {
                    Fail("Minigame not found: " + asked);
                    return;
                }
                if (!string.IsNullOrEmpty(config.mechanic) && config.mechanic != game.Id) {
                    Fail("The minigame " + asked + " is a " + config.mechanic + ", this build plays " + game.Id);
                    return;
                }
                game.Apply(config);
                Play(asked);
            });
        }

        void Play(string gameId) {
            // Same as the minigames button of the app
            UIManager.Instance.MiniGames();
            WebMiniGamesBridge.Notify("opened", JsonConvert.SerializeObject(new { game = gameId }));
        }

        void Fail(string message) {
            Debug.LogError("[WebMiniGames] " + message);
            WebMiniGamesBridge.Notify("error", JsonConvert.SerializeObject(new { message }));
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
