using System.Collections;
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
            WebMiniGamesBridge.Notify("loading");
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

        IEnumerator OpenNextFrame() {
            yield return null;
            // Same as the minigames button of the app
            UIManager.Instance.MiniGames();
            WebMiniGamesBridge.Notify("opened");
        }

        void Fail(string message) {
            Debug.LogError("[WebMiniGames] " + message);
            WebMiniGamesBridge.Notify("error", JsonConvert.SerializeObject(new { message }));
        }
    }
}
