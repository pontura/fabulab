using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Yaguar.FirebaseRest
{
    /// <summary>
    /// Session given by the web page that embeds this WebGL player (web/client signs in with the Firebase JS
    /// SDK and opens the Unity editor in an iframe of the same origin). The page sets
    ///   window.fabulabHostSession = { refreshToken, uid, email }
    /// before loading the iframe, so the player starts signed in as the same user (WebGL/HostSession.jslib).
    /// </summary>
    public static class FirebaseRestHostSession
    {
        [Serializable]
        class HostSession
        {
            public string refreshToken;
            public string uid;
            public string email;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern string FabulabGetHostSession();
#endif

        /// <summary>Adopts the session of the host page, if it gives one. True when it did.</summary>
        public static bool TryAdopt()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                string json = FabulabGetHostSession();
                if (string.IsNullOrEmpty(json))
                    return false;
                HostSession session = JsonUtility.FromJson<HostSession>(json);
                if (session == null || string.IsNullOrEmpty(session.refreshToken) || string.IsNullOrEmpty(session.uid))
                    return false;
                FirebaseRestSession.Adopt(session.refreshToken, session.uid, session.email);
                Debug.Log("#Firebase REST session given by the host page: " + session.uid);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("#FirebaseRestHostSession failed: " + e.Message);
                return false;
            }
#else
            return false;
#endif
        }
    }
}
