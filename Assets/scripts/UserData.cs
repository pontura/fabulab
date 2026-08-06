using BoardItems.BoardData;
using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.StoryMaker.DB;

public class UserData : MonoBehaviour
{
    public UserDataInDatabase userDataInDatabase;
    [Serializable]
    public class UserDataInDatabase : FirebaseAuthManager.UserDataInDatabase
    {
        public string deviceID;
        public List<string> likes;
    }

    public int onboardingSteps;
    public bool isAdmin;
    public bool passport;

    public List<int> prizes;

    public bool UserDataLoadedDone {  get; private set; }

    void Awake() {
        onboardingSteps = PlayerPrefs.GetInt("onboardingSteps", 0);
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
    public void OnBoardingAllStepsDone()
    {
        PlayerPrefs.SetInt("onboardingSteps", 1);
        onboardingSteps = 1;
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
                        Events.OnSimplePopup("�UPS!", "Parece que necesit�s volver a ingresar tu usuario y contrase�a");
                        Data.Instance.LoadLevel("Login");
                    }
                });
            } else {
                Events.OnSimplePopup("�UPS!", "Parece que necesit�s volver a ingresar tu usuario y contrase�a");
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
            FirebaseStoryMakerDBManager.Instance.LoadUserLikeFromServer(OnLoadingUserLikesFromServer);
        } else
            Invoke("OnTokenUpdated", 1);         
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
        userDataInDatabase.likes = new();
        onboardingSteps = 0;
        UserDataLoadedDone = false;
    }

    void OnLoadingUserLikesFromServer(List<string> l) {
        if (l!=null)
            userDataInDatabase.likes = l;
        UserDataLoadedDone = true;
        Events.OnAllUserDataLoadDone();
    }

    public bool isLiked(string filmId) {
        if (userDataInDatabase.likes != null)
            return userDataInDatabase.likes.Contains(filmId);
        else
            return false;
    }

    public void OnLikeUpdate(MetadataTypes type, string id, bool adding) {
        if (!isLiked(id) && adding) {
            if (userDataInDatabase.likes == null)
                userDataInDatabase.likes = new List<string>();
            AddLike(type, id);                
        } else if(isLiked(id) && !adding) {
            RemoveLike(type, id);
        }
    }

    void AddLike(MetadataTypes type, string id) {
        userDataInDatabase.likes.Add(id);
        FirebaseStoryMakerDBManager.Instance.AddUserLikeToServer(id);        
        FirebaseStoryMakerDBManager.Instance.AddLikeCountToFilm(type.ToString(),id);
    }

    void RemoveLike(MetadataTypes type, string id) {
        userDataInDatabase.likes.Remove(id);
        FirebaseStoryMakerDBManager.Instance.RemoveUserLikeToServer(id);
        FirebaseStoryMakerDBManager.Instance.RemoveLikeCountToFilm(type.ToString(), id);
    }
}
