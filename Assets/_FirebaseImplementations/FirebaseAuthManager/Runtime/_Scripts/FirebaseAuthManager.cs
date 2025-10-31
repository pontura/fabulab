using UnityEngine;
using System;
using Firebase.Auth;
using Yaguar.DB;
using System.Threading.Tasks;

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
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
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
            }, taskScheduler);

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

        public void SignInWithPlayGames(string authCode, System.Action<bool> callback)
        {
            Debug.Log("#SignInWithPlayGames:  " + authCode);
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Firebase.Auth.Credential credential = Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
            _auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
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

                IFirebaseDBManager dBManager = firebaseDBManager.GetComponent<IFirebaseDBManager>();

                Debug.Log(dBManager);

                dBManager.GetInstance().LoadUserData(result.User.UserId, (username, email, uid) =>
                {
                    Debug.Log("#CheckUserExist callback uid: " + uid);
                    if (uid != "" && uid.Length > 1)
                    {
                        Debug.Log("# uid!=void");
                        OnFirebaseAuthenticated.Invoke(username, email, uid);
                        callback(true);
                    }
                    else
                    {
                        Debug.Log("# uid==void");
                        OnFirebaseAuthenticated?.Invoke(result.User.DisplayName, "", result.User.UserId);
                        print("Signed Up localId GooglePlay: " + result.User.UserId);
                        UserDataInDatabase udata = new UserDataInDatabase();
                        udata.username = result.User.DisplayName;
                        udata.uid = result.User.UserId;
                        dBManager.GetInstance().SaveUserToServer(udata);
                        //GetServerTime();
                        callback(true);
                        OnSignUp?.Invoke(true);
                    }
                });

                /*dBManager.GetInstance().CheckUserExist(result.User.UserId, (exist) =>
                {
                    Debug.Log("#CheckUserExist callback: "+exist);
                    if (exist)
                    {
                        dBManager.GetInstance().LoadUserData(result.User.UserId, OnFirebaseAuthenticated);
                    }
                    else
                    {
                        OnFirebaseAuthenticated?.Invoke(result.User.DisplayName, "", result.User.UserId);
                        print("Signed Up localId GooglePlay: " + result.User.UserId);
                        UserDataInDatabase udata = new UserDataInDatabase();
                        udata.username = result.User.DisplayName;
                        udata.uid = result.User.UserId;
                        udata.deviceID = SystemInfo.deviceUniqueIdentifier;
                        dBManager.GetInstance().SaveUserToServer(udata);
                        //GetServerTime();
                        OnSignUp?.Invoke(true);
                        callback(true);
                    }
                });*/


                /*firebaseDBManager.GetComponent<IFirebaseDBManager>().LoadUserData(result.User.UserId, (username, email, uid)=> {
                    Debug.Log("#ACA uid: " + uid);
                    if (uid != "")
                    {
                        OnFirebaseAuthenticated?.Invoke(username, email, uid);                        
                        OnLogin?.Invoke(true);
                        callback(true);
                    }
                    else
                    {
                        Debug.Log("#ACA 2");
                        OnFirebaseAuthenticated?.Invoke(result.User.DisplayName, email, result.User.UserId);
                        print("Signed Up localId GooglePlay: " + result.User.UserId);
                        UserDataInDatabase udata = new UserDataInDatabase();
                        udata.username = username;
                        udata.uid = result.User.UserId;
                        udata.deviceID = SystemInfo.deviceUniqueIdentifier;
                        firebaseDBManager.GetComponent<IFirebaseDBManager>().SaveUserToServer(udata);
                        //GetServerTime();
                        OnSignUp?.Invoke(true);
                        callback(true);
                    }
                });*/
            }, taskScheduler);
        }

        public void SignUpUserEmail(string username, string email, string password)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
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

                OnFirebaseAuthenticated?.Invoke(username, email, result.User.UserId);
                print("Signed Up localId: " + result.User.UserId);
                UserDataInDatabase udata = new UserDataInDatabase();
                udata.username = username;
                udata.email = email;
                udata.uid = result.User.UserId;
                firebaseDBManager.GetComponent<IFirebaseDBManager>().SaveUserToServer(udata);
                //GetServerTime();
                OnSignUp?.Invoke(true);
            }, taskScheduler);
            Debug.Log("Server: SignUpUserEmail");
        }
        public void LoginUserByEmail(string email, string password)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    OnLogin?.Invoke(false);
                    return;
                }

                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);

                firebaseDBManager.GetComponent<IFirebaseDBManager>().LoadUserData(result.User.UserId, OnFirebaseAuthenticated);
                OnLogin?.Invoke(true);
            }, taskScheduler);
            Debug.Log("Server: LoginUserByEmail");
        }

        public void PasswordReset(string email)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _auth.SendPasswordResetEmailAsync(email).ContinueWith(task => {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + task.Exception);
                    OnResetPassword?.Invoke(false);
                    return;
                }

                Debug.Log("Email reseted: " + email);
                OnResetPassword?.Invoke(true);
            }, taskScheduler);
            Debug.Log("Server: PasswordReset");
        }
    }
}
