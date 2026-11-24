using System.Runtime.InteropServices;
using UnityEngine;

namespace WebEditor
{
    // Messages from the editor to the web page that embeds it (see Plugins/WebGL/WebEditorBridge.jslib).
    // Outside WebGL they are only logged
    public static class WebEditorBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void FabulabEditorNotify(string eventName, string payloadJson);
#endif

        public static void Notify(string eventName, string payloadJson = "{}") {
#if UNITY_WEBGL && !UNITY_EDITOR
            FabulabEditorNotify(eventName, payloadJson ?? "{}");
#else
            Debug.Log("[WebEditor] " + eventName + " " + payloadJson);
#endif
        }
    }
}
