using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Yaguar.Auth;

public class UserData : MonoBehaviour
{
    //[SerializeField] SocialAuth socialAuth;

    public UserDataInDatabase userDataInDatabase;
    [Serializable]
    public class UserDataInDatabase : FirebaseAuthManager.UserDataInDatabase
    {
        public string deviceID;
    }

    public bool isAdmin;
    public bool passport;

    public List<int> prizes;

    void Awake() {

        userDataInDatabase.username = PlayerPrefs.GetString("username", "");
        userDataInDatabase.email = PlayerPrefs.GetString("email", "");
        userDataInDatabase.uid = PlayerPrefs.GetString("uid", "");
        userDataInDatabase.deviceID = PlayerPrefs.GetString("deviceID", "");

        /*Social.localUser.Authenticate(success => {
            if (success) {
                Debug.Log("Authentication successful");
                string userInfo = "Username: " + Social.localUser.userName +
                    "\nUser ID: " + Social.localUser.id +
                    "\nIsUnderage: " + Social.localUser.underage;
                Debug.Log(userInfo);
            } else
                Debug.Log("Authentication failed");
        });*/

    }
    private void Start() {               

        FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
        FirebaseAuthManager.Instance.OnSignedOut += OnVerifyTokenFail;
        FirebaseAuthManager.Instance.OnFirebaseAuthenticated += SaveUser;

        Invoke(nameof(CheckAdmin), Time.deltaTime * 3);
    }

    void CheckAdmin() {
        isAdmin = Data.Instance.adminData.IsAdmin(userDataInDatabase.uid);
        if (isAdmin)
            passport = true;
    }

    void OnVerifyTokenFail() {
        Debug.Log("#OnVerifyTokenFail");
        ResetUserData();

        /*socialAuth.Init((authCode) => {
            Debug.Log("#socialAuth: " + authCode);
            if (authCode != "") {
                FirebaseAuthManager.Instance.SignInWithPlayGames(authCode, (success) => {
                    if (!success) {
                        Events.OnSimplePopup("¡UPS!", "Parece que necesitás volver a ingresar tu usuario y contraseña");
                        Data.Instance.LoadLevel("Login");
                    }
                });
            } else {
                Events.OnSimplePopup("¡UPS!", "Parece que necesitás volver a ingresar tu usuario y contraseña");
                Data.Instance.LoadLevel("Login");
            }
        });*/
    }    

    private void OnDestroy() {       

        FirebaseAuthManager.Instance.OnSignedOut -= OnVerifyTokenFail;
        FirebaseAuthManager.Instance.OnFirebaseAuthenticated -= SaveUser;
        FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
    }

    void OnTokenUpdated() {
        //ResetUserData();
        if (IsLogged()) {
            CheckAdmin();
        }
    }

    void OnPassport(bool isPassport) {
        passport = isPassport;
    }
    public bool IsLogged() {
        if (userDataInDatabase.uid.Length == 0)
            return false;
        return true;
    }

    public void Register(string username, string email, string uid) {
        SaveUser(username, email, uid);
    }

    public void SaveUser(string username, string email, string uid) {
        Debug.Log("#SaveUser: " + username);
        userDataInDatabase.deviceID = SystemInfo.deviceUniqueIdentifier;
        userDataInDatabase.username = username;
        userDataInDatabase.email = email;
        userDataInDatabase.uid = uid;

        PlayerPrefs.SetString("deviceID", userDataInDatabase.deviceID);
        PlayerPrefs.SetString("username", userDataInDatabase.username);
        PlayerPrefs.SetString("email", userDataInDatabase.email);
        PlayerPrefs.SetString("uid", userDataInDatabase.uid);

        isAdmin = Data.Instance.adminData.IsAdmin(userDataInDatabase.uid);
        if (isAdmin)
            passport = true;

    }
    
    public void ResetUserData() {
        Debug.Log("#ResetUserData");
        PlayerPrefs.DeleteAll();
        userDataInDatabase.username = "";
        userDataInDatabase.email = "";
        userDataInDatabase.uid = "";
    }
}
