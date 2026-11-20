using System.Runtime.InteropServices;
using UnityEngine;

namespace StoryPlayer
{
    // Messages from the story player to the HTML page that embeds it (see Plugins/WebGL/StoryPlayerBridge.jslib).
    // Outside WebGL they are only logged
    public static class StoryPlayerWebBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void FabulabStoryPlayerNotify(string eventName, string payloadJson);
#endif

        public static void Notify(string eventName, string payloadJson = "{}") {
#if UNITY_WEBGL && !UNITY_EDITOR
            FabulabStoryPlayerNotify(eventName, payloadJson ?? "{}");
#else
            Debug.Log("[StoryPlayer] " + eventName + " " + payloadJson);
#endif
        }
    }
}
