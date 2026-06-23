using UnityEngine;
using System;
using Firebase.Extensions;
using Firebase.Auth;
using Yaguar.DB;
using System.Threading.Tasks;
using Firebase.Extensions;

namespace Yaguar.Auth
{
    public class FirebaseAuthManager : MonoBehaviour
    {
        public static FirebaseAuthManager Instance { get { return mInstance; } }
        static FirebaseAuthManager mInstance = null;

        [SerializeField] GameObject firebaseDBManager;

        FirebaseAuth _auth;
        FirebaseUser _user;

        public class UserDataInDatabase
        {
            public string username;
            public string email;
            public string uid;
        }

        public FirebaseUser User()
        {
            return _user;
        }
        public FirebaseAuth Auth()
        {
            return _auth;
        }

        int verifyTokenCount;
        int verifyTokenMaxCount = 5;

        #region Events
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
                UnityEngine.Debug.LogError($"There should be only one {nameof(FirebaseAuthManager)} in the Scene!. Destroying...");
                Destroy(gameObject);
                return;
            }

            mInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            //var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    Debug.Log("#Firebase Ready");
                    _auth = FirebaseAuth.GetAuth(Firebase.FirebaseApp.DefaultInstance);
                    _auth.StateChanged += AuthStateChanged;
                    //AuthStateChanged(this, null);
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });

        }

        void AuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            Debug.Log("#AuthStateChanged");
            if (_auth.CurrentUser != _user)
            {
                bool signedIn = _user != _auth.CurrentUser && _auth.CurrentUser != null
                    && _auth.CurrentUser.IsValid();
                if (!signedIn && _user != null)
                {
                    OnSignedOut?.Invoke();
                    Debug.Log("Signed out " + _user.UserId);
                }
                _user = _auth.CurrentUser;
                if (signedIn)
                {
                    Debug.Log("Signed in " + _user.UserId);
                    OnTokenUpdated?.Invoke();
                }
            }
            else if (_auth.CurrentUser == null || _auth.CurrentUser.IsValid())
            {
                OnSignedOut?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (_auth != null)
                _auth.StateChanged -= AuthStateChanged;
            _auth = null;

            OnFirebaseAuthenticated = null;
            OnSignedOut = null;
            OnTokenUpdated = null;
        }

        public void SignOut()
        {
            _auth?.SignOut();
        }

        public bool IsAnonymousUser() {
            return FirebaseAuth.DefaultInstance.CurrentUser.IsAnonymous;
        }

        public void SignInWithPlayGames(string authCode, System.Action<bool> callback)
        {
            Debug.Log("#SignInWithPlayGames:  " + authCode);
            Firebase.Auth.Credential credential = Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
            _auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task => {
                if (task.IsCanceled)
                {
                    Debug.Log("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                    callback(false);
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.Log("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                    callback(false);
                    return;
                }

                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);

                OnSignedUpWithPlayGames(result.User.DisplayName, result.User.UserId, callback);                             
            });
        }

        public void LinkWithPlayGames(string username, string authCode, System.Action<bool> callback) { 
            Debug.Log("#LinkWithPlayGames:  " + authCode);
            Firebase.Auth.Credential credential = Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
            _user.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    Debug.Log("LinkWithPlayGames was canceled.");
                    callback(false);
                    return;
                }
                if (task.IsFaulted) {
                    Debug.Log("LinkWithPlayGames encountered an error: " + task.Exception);
                    callback(false);
                    return;
                }

                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    username, result.User.UserId);

                OnSignedUpWithPlayGames(username, result.User.UserId, callback);

            });                                      
        }

        void OnSignedUpWithPlayGames(string displayName, string userId, System.Action<bool> callback) {
            IFirebaseDBManager dBManager = firebaseDBManager.GetComponent<IFirebaseDBManager>();

            dBManager.GetInstance().LoadUserData(userId, (username, email, uid) => {
                Debug.Log("#CheckUserExist callback uid: " + uid);
                if (uid != "" && uid.Length > 1 && username == "" && username.Length > 1) {
                    Debug.Log("# uid!=void");
                    OnFirebaseAuthenticated.Invoke(username, email, uid);
                    callback(true);
                } else {
                    Debug.Log("# uid==void or username==void");
                    OnFirebaseAuthenticated?.Invoke(displayName, email, userId);
                    print("Signed Up localId GooglePlay: " + userId);
                    UserDataInDatabase udata = new UserDataInDatabase();
                    udata.username = displayName;
                    udata.email = email;
                    udata.uid = userId;
                    dBManager.GetInstance().SaveUserToServer(udata);
                    //GetServerTime();
                    callback(true);
                    OnSignUp?.Invoke(true);
                }
            });
        }

        public void SignInAnonymously() {
            _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
                if (task.IsCanceled || task.IsFaulted) {
                    Debug.LogError("Error en login anónimo: " + task.Exception);
                    OnSignUp?.Invoke(false);
                    return;
                }
                // Firebase user has been created.
                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);

                OnFirebaseAuthenticated?.Invoke("", "", result.User.UserId);
                print("Signed Up localId: " + result.User.UserId);
                UserDataInDatabase udata = new UserDataInDatabase();
                udata.username = "";
                udata.email = "";
                udata.uid = result.User.UserId;
                firebaseDBManager.GetComponent<IFirebaseDBManager>().SaveUserToServer(udata);
                //GetServerTime();
                OnSignUp?.Invoke(true);
            });

            Debug.Log("Server: SignInAnonymously");
        }

        public void LinkWithEmail(string username, string email, string password) {
            Credential credential = EmailAuthProvider.GetCredential(email, password);

            _user.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task => {
                if (task.IsCanceled || task.IsFaulted) {
                    Debug.LogError("Error al vincular email: " + task.Exception);
                    OnSignUp?.Invoke(false);
                    return;
                }
                Firebase.Auth.AuthResult result = task.Result;
                //FirebaseUser newUser = task.Result;
                Debug.Log("Cuenta vinculada con email: " + result.User.Email);

                OnEmailSignedUp(username, email, result.User.UserId);
            });
        }
        public void SignUpUserEmail(string username, string email, string password)
        {
            _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.Log("ERROR: " + task.Exception);
                    OnSignUp?.Invoke(false);
                    return;
                }

                // Firebase user has been created.
                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);

                OnEmailSignedUp(username, email, result.User.UserId);
            });
            Debug.Log("Server: SignUpUserEmail");
        }

        void OnEmailSignedUp(string username, string email, string uid) {
            OnFirebaseAuthenticated?.Invoke(username, email, uid);
            print("Signed Up localId: " + uid);
            UserDataInDatabase udata = new UserDataInDatabase();
            udata.username = username;
            udata.email = email;
            udata.uid = uid;
            firebaseDBManager.GetComponent<IFirebaseDBManager>().SaveUserToServer(udata);
            //GetServerTime();
            OnSignUp?.Invoke(true);
        }
        public void LoginUserByEmail(string email, string password)
        {
            _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    OnLogin?.Invoke(false);
                    return;
                }

                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
                OnLogin?.Invoke(true);
                firebaseDBManager.GetComponent<IFirebaseDBManager>().LoadUserData(result.User.UserId, OnFirebaseAuthenticated);
                
            });
            Debug.Log("Server: LoginUserByEmail");
        }

        public void PasswordReset(string email)
        {
            _auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task => {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + task.Exception);
                    OnResetPassword?.Invoke(false);
                    return;
                }

                Debug.Log("Email reseted: " + email);
                OnResetPassword?.Invoke(true);
            });
            Debug.Log("Server: PasswordReset");
        }
    }
}
