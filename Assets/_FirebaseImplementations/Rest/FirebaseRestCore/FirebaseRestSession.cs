using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Yaguar.FirebaseRest
{
    /// <summary>
    /// Firebase Auth over the Identity Toolkit REST API. Holds the current user, persists the refresh token
    /// and hands out a valid id token (refreshing it transparently) to the Database / Storage / Functions clients.
    /// </summary>
    public static class FirebaseRestSession
    {
        const string PrefsKey = "fbrest_session";
        const string IdentityUrl = "https://identitytoolkit.googleapis.com/v1/accounts:";
        const string RefreshUrl = "https://securetoken.googleapis.com/v1/token";
        const int RefreshMarginSeconds = 60;

        [Serializable]
        class StoredSession
        {
            public string refreshToken;
            public string uid;
            public string email;
            public bool anonymous;
        }

        [Serializable]
        class AuthResponse
        {
            public string idToken;
            public string refreshToken;
            public string expiresIn;
            public string localId;
            public string email;
        }

        [Serializable]
        class RefreshResponse
        {
            public string id_token;
            public string refresh_token;
            public string expires_in;
            public string user_id;
        }

        static string idToken;
        static string refreshToken;
        static DateTime expiresAtUtc = DateTime.MinValue;
        static Task<string> refreshing;

        public static string Uid { get; private set; }
        public static string Email { get; private set; }
        public static bool IsAnonymous { get; private set; }
        public static bool IsSignedIn { get { return !string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(refreshToken); } }

        /// <summary>Raised when the backend rejects the stored session (revoked / deleted user).</summary>
        public static event Action SessionInvalidated;

        #region Persistence
        /// <summary>Loads the stored session, if any. Returns true when a user was restored.</summary>
        public static bool Restore()
        {
            try
            {
                string json = PlayerPrefs.GetString(PrefsKey, "");
                if (string.IsNullOrEmpty(json))
                    return false;
                var s = JsonUtility.FromJson<StoredSession>(json);
                if (s == null || string.IsNullOrEmpty(s.refreshToken) || string.IsNullOrEmpty(s.uid))
                    return false;
                refreshToken = s.refreshToken;
                Uid = s.uid;
                Email = s.email;
                IsAnonymous = s.anonymous;
                idToken = null;
                expiresAtUtc = DateTime.MinValue;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("#FirebaseRestSession Restore failed: " + e.Message);
                return false;
            }
        }

        static void Persist()
        {
            var s = new StoredSession { refreshToken = refreshToken, uid = Uid, email = Email, anonymous = IsAnonymous };
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(s));
            PlayerPrefs.SetString("refreshToken", refreshToken ?? "");
            PlayerPrefs.Save();
        }

        static void Clear()
        {
            idToken = null;
            refreshToken = null;
            expiresAtUtc = DateTime.MinValue;
            Uid = null;
            Email = null;
            IsAnonymous = false;
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.DeleteKey("refreshToken");
            PlayerPrefs.Save();
        }

        public static void SignOut()
        {
            Clear();
        }
        #endregion

        #region Token
        /// <summary>Returns a valid id token, or null when nobody is signed in.</summary>
        public static Task<string> GetIdTokenAsync()
        {
            if (!IsSignedIn)
                return Task.FromResult<string>(null);
            if (!string.IsNullOrEmpty(idToken) && DateTime.UtcNow < expiresAtUtc.AddSeconds(-RefreshMarginSeconds))
                return Task.FromResult(idToken);
            if (refreshing == null)
                refreshing = RefreshAsync();
            return refreshing;
        }

        static async Task<string> RefreshAsync()
        {
            try
            {
                string body = "grant_type=refresh_token&refresh_token=" + Uri.EscapeDataString(refreshToken);
                var res = await FirebaseRestHttp.SendAsync("POST", RefreshUrl + "?key=" + FirebaseRestConfig.ApiKey,
                    System.Text.Encoding.UTF8.GetBytes(body), "application/x-www-form-urlencoded");

                if (res.Ok)
                {
                    var r = JsonUtility.FromJson<RefreshResponse>(res.Text);
                    idToken = r.id_token;
                    refreshToken = r.refresh_token;
                    expiresAtUtc = DateTime.UtcNow.AddSeconds(ParseSeconds(r.expires_in));
                    Persist();
                    return idToken;
                }

                if (!res.IsNetworkError && res.Status >= 400 && res.Status < 500)
                {
                    // TOKEN_EXPIRED / USER_DISABLED / USER_NOT_FOUND / INVALID_REFRESH_TOKEN: the session is gone.
                    Debug.LogWarning("#FirebaseRestSession refresh rejected: " + res.Text);
                    Clear();
                    SessionInvalidated?.Invoke();
                    return null;
                }

                Debug.LogWarning("#FirebaseRestSession refresh failed (will retry on next request): " + res.Status);
                return idToken;
            }
            finally
            {
                refreshing = null;
            }
        }

        static double ParseSeconds(string s)
        {
            double v;
            return double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v) ? v : 3600;
        }
        #endregion

        #region Sign in / up
        public static Task SignInAnonymouslyAsync()
        {
            return AuthCallAsync("signUp", "{\"returnSecureToken\":true}", true);
        }

        public static Task SignUpWithEmailAsync(string email, string password)
        {
            return AuthCallAsync("signUp", CredentialsJson(email, password, null), false);
        }

        public static Task SignInWithEmailAsync(string email, string password)
        {
            return AuthCallAsync("signInWithPassword", CredentialsJson(email, password, null), false);
        }

        /// <summary>Upgrades the current (anonymous) user to an email/password account, keeping its uid.</summary>
        public static async Task LinkEmailAsync(string email, string password)
        {
            string token = await GetIdTokenAsync();
            if (token == null)
                throw new FirebaseRestException("LinkEmail: no signed in user");
            await AuthCallAsync("update", CredentialsJson(email, password, token), false);
        }

        public static async Task SendPasswordResetAsync(string email)
        {
            string body = "{\"requestType\":\"PASSWORD_RESET\",\"email\":" + JsonString(email) + "}";
            var res = await FirebaseRestHttp.SendAsync("POST", IdentityUrl + "sendOobCode?key=" + FirebaseRestConfig.ApiKey,
                System.Text.Encoding.UTF8.GetBytes(body), "application/json");
            if (!res.Ok)
                throw FirebaseRestException.From("PasswordReset", res);
        }

        static async Task AuthCallAsync(string endpoint, string body, bool anonymous)
        {
            var res = await FirebaseRestHttp.SendAsync("POST", IdentityUrl + endpoint + "?key=" + FirebaseRestConfig.ApiKey,
                System.Text.Encoding.UTF8.GetBytes(body), "application/json");
            if (!res.Ok)
                throw FirebaseRestException.From("Auth " + endpoint, res);

            var r = JsonUtility.FromJson<AuthResponse>(res.Text);
            idToken = r.idToken;
            refreshToken = r.refreshToken;
            expiresAtUtc = DateTime.UtcNow.AddSeconds(ParseSeconds(r.expiresIn));
            Uid = r.localId;
            Email = r.email;
            IsAnonymous = anonymous;
            Persist();
        }

        static string CredentialsJson(string email, string password, string idTokenForLink)
        {
            string s = "{\"email\":" + JsonString(email) + ",\"password\":" + JsonString(password) + ",\"returnSecureToken\":true";
            if (idTokenForLink != null)
                s += ",\"idToken\":" + JsonString(idTokenForLink);
            return s + "}";
        }

        static string JsonString(string value)
        {
            if (value == null)
                return "null";
            var sb = new System.Text.StringBuilder("\"");
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.Append('"').ToString();
        }
        #endregion
    }
}
