using System.Runtime.InteropServices;
using UnityEngine;

namespace WebMiniGames
{
    // Messages from the minigames to the web page that embeds them (see Plugins/WebGL/WebMiniGamesBridge.jslib).
    // Outside WebGL they are only logged
    public static class WebMiniGamesBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void FabulabMiniGamesNotify(string eventName, string payloadJson);
#endif

        public static void Notify(string eventName, string payloadJson = "{}") {
#if UNITY_WEBGL && !UNITY_EDITOR
            FabulabMiniGamesNotify(eventName, payloadJson ?? "{}");
#else
            Debug.Log("[WebMiniGames] " + eventName + " " + payloadJson);
#endif
        }
    }
}
