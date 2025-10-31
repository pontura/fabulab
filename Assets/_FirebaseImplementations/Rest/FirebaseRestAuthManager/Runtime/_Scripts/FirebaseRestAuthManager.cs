using System.Collections;
using Proyecto26;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace Yaguar.Auth
{
    public class FirebaseRestAuthManager : MonoBehaviour
    {
        public static FirebaseRestAuthManager Instance { get { return mInstance; } }
        static FirebaseRestAuthManager mInstance = null;

        [SerializeField] string token;
        [SerializeField] string refreshToken;

        [Serializable]
        public class SignResponse
        {
            public string localId;
            public string email;
            public string idToken;
            public string refreshToken;
            public string expiresIn;
        }

        [Serializable]
        public class LoginResponse
        {
            public string idToken;
            public string email;
            public string refreshToken;
            public string expiresIn;
            public string localId;
            public bool registered;
        }

        [Serializable]
        public class RefreshData
        {
            public string refresh_token;
            public string id_token;
        }

        [Serializable]
        public class ResetEmail
        {
            public string email;
        }

        public class UserDataInDatabase
        {
            public string username;
            public string email;
            public string uid;
            public string deviceID;
            public int score;
        }

        [SerializeField] private string databaseURL;
        [SerializeField] private string AuthKey;

        int verifyTokenCount;
        int verifyTokenMaxCount = 5;

        #region Events
        public event Action<string> OnFirebaseInit;
        public event Action<string, string> OnFirebaseReady;
        public event Action<string, string, string> OnFirebaseAuthenticated;
        public event Action OnTokenUpdated;
        public event Action OnSignedOut;
        public event Action<bool> OnSignUp;
        public event Action<bool> OnLogin;
        public event Action<bool> OnResetPassword;        
        #endregion

        private void Awake()
        {
            if (Instance != null)
            {
                UnityEngine.Debug.LogError($"There should be only one {nameof(FirebaseRestAuthManager)} in the Scene!. Destroying...");
                Destroy(gameObject);
                return;
            }

            mInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            OnFirebaseInit?.Invoke(databaseURL);
            token = PlayerPrefs.GetString("token", "");
            refreshToken = PlayerPrefs.GetString("refreshToken", "");

            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    //   app = Firebase.FirebaseApp.DefaultInstance;

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                    OnFirebaseReady?.Invoke(databaseURL, token);
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });

            if (refreshToken != "")
                Invoke(nameof(VerifyToken), 2);
        }

        private void OnDestroy()
        {
            OnFirebaseInit = null;
            OnFirebaseReady = null;
            OnFirebaseAuthenticated = null;
            OnSignedOut = null;
            OnTokenUpdated = null;
        }

        void SaveToken(string token)
        {
            PlayerPrefs.SetString("token", token);
            this.token = token;
        }
        void SaveRefreshToken(string token)
        {
            PlayerPrefs.SetString("refreshToken", token);
            this.refreshToken = token;
        }

        public void VerifyToken()
        {
            StartCoroutine(Upload());
        }
        IEnumerator Upload()
        {
            WWWForm form = new WWWForm();
            form.AddField("grant_type", "refresh_token");
            form.AddField("refresh_token", refreshToken);

            form.headers["Content-Type"] = "application/x-www-form-urlencoded";
            UnityWebRequest www = UnityWebRequest.Post("https://securetoken.googleapis.com/v1/token?key=" + AuthKey, form);

            yield return www.SendWebRequest();

            if (www.isHttpError)
            {
                Debug.Log("HTTP ERROR");
                Debug.Log(www.error);
                verifyTokenCount++;
                if (verifyTokenCount >= verifyTokenMaxCount)
                {
                    OnSignedOut?.Invoke();                    
                }
                else
                    Invoke(nameof(VerifyToken), 2);
            }
            else if (www.isNetworkError)
            {
                Debug.Log("NETWORK ERROR");
                Debug.Log(www.error);
                Invoke(nameof(VerifyToken), 2);
            }
            else
            {
                RefreshData response = JsonUtility.FromJson<RefreshData>(www.downloadHandler.text);
                Debug.Log("Server: update tokens");
                /*Debug.Log("new id_token    " + response.id_token);
                Debug.Log("old id_token    " + Data.Instance.userData.token);

                Debug.Log("new refresh_token       " + response.refresh_token);
                Debug.Log("old freshTokend_token   " + Data.Instance.userData.refreshToken);*/

                SaveToken(response.id_token);
                SaveRefreshToken(response.refresh_token);

                OnFirebaseReady?.Invoke(databaseURL, response.id_token);
                OnTokenUpdated?.Invoke();

            }
        }

        public void SignUpUserEmail(string username, string email, string password)
        {
            string userData = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";
            //Debug.Log("https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=" + AuthKey + " : " + userData);        
            RestClient.Post<SignResponse>("https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=" + AuthKey, userData).Then(
                response =>
                {
                //Debug.Log("email:" + response.email + " - expiresIn:" + response.expiresIn + " ID TOKEN: " + response.idToken + " localID: " + response.localId + " refresh Token:" + response.refreshToken);
                    SaveToken(response.idToken);
                    SaveRefreshToken(response.refreshToken);
                    OnFirebaseAuthenticated?.Invoke(username, response.email, response.localId);
                    OnFirebaseReady?.Invoke(databaseURL, response.idToken);
                    print("LOGUEADO localId: " + response.localId);
                    UserDataInDatabase udata = new UserDataInDatabase();
                    udata.username = username;
                    udata.email = email;
                    udata.uid = response.localId;
                    udata.deviceID = SystemInfo.deviceUniqueIdentifier;
                    OnSaveUserToServer(udata);
                //GetServerTime();
                    OnSignUp?.Invoke (true);
                }).Catch(error =>
                {
                    Debug.Log("ERROR: " + error);
                    OnSignUp?.Invoke(false);
                });
            Debug.Log("Server: SignUpUserEmail");
        }
        public void LoginUserByEmail(string email, string password)
        {
            string userData = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";
            //Debug.Log("https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=" + AuthKey + " : " + userData);
            RestClient.Post<LoginResponse>("https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key=" + AuthKey, userData).Then(
                response =>
                {
                    SaveToken(response.idToken);
                    SaveRefreshToken(response.refreshToken);
                    OnFirebaseReady?.Invoke(databaseURL, response.idToken);                                    
                    OnUserLogin(response.localId);
                    OnLogin?.Invoke(true);
                }).Catch(error =>
                {
                    Debug.Log(error);
                    OnLogin?.Invoke(false);
                });
            Debug.Log("Server: LoginUserByEmail");
        }

        public void PasswordReset(string email)
        {
            string userData = "{\"requestType\":\"PASSWORD_RESET\",\"email\":\"" + email + "\"}";
            //Debug.Log("https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=" + AuthKey + " : " + userData);
            RestClient.Post<ResetEmail>("https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=" + AuthKey, userData).Then(
                response =>
                {
                    Debug.Log("Email reseted: " + response.email);
                    OnResetPassword?.Invoke(true);
                //GetServerTime();
            }).Catch(error =>
            {
                Debug.Log("ERROR: " + error);
                OnResetPassword?.Invoke(false);
            });
            Debug.Log("Server: PasswordReset");
        }

        public void OnSaveUserToServer(UserDataInDatabase userData)
        {
            string url = databaseURL + "/users/" + userData.uid + "/.json?auth=" + token;
            RestClient.Put(url, userData).Then(
                response =>
                {
                    Debug.Log("Response: " + response.Text);

                }).Catch(error =>
                {
                    Debug.Log(error);
                });
            Debug.Log("Server: OnSaveUserToServer");
            //print("OnSaveUserDate url : " + url);
        }

        public void OnUserLogin(string uid)
        {
            string url = databaseURL + "/users/" + uid + "/.json?auth=" + token;
            RestClient.Get<UserDataInDatabase>(url).Then(
                response =>
                {
                    OnFirebaseAuthenticated?.Invoke(response.username, response.email, response.uid);
                }).Catch(error =>
                {
                    Debug.Log(error);
                });
            Debug.Log("Server: OnUserLogin");
            //print("OnUserLogin url : " + url);
        }
    }
}
