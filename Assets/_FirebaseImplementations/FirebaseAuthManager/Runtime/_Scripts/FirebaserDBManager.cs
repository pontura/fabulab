using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using Yaguar.Auth;

namespace Yaguar.DB
{
    public class FirebaseDBManager : MonoBehaviour, IFirebaseDBManager
    {
        public static FirebaseDBManager Instance { get { return mInstance; } }
        static FirebaseDBManager mInstance = null;


        [SerializeField] protected string _uid;

        private void Awake()
        {
            if (Instance != null)
            {
                UnityEngine.Debug.LogError($"There should be only one {nameof(FirebaseDBManager)} in the Scene!. Destroying...");
                Destroy(gameObject);
                return;
            }
            mInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        void Start()
        {
            FirebaseAuthManager.Instance.OnFirebaseAuthenticated += OnFirebaseAuthenticated;
        }

        private void OnDestroy()
        {
            FirebaseAuthManager.Instance.OnFirebaseAuthenticated += OnFirebaseAuthenticated;
        }

        void OnFirebaseAuthenticated(string username, string email, string uid)
        {
            Debug.Log("#OnFirebaseAuthenticated");
            _uid = uid;
        }

        public virtual IFirebaseDBManager GetInstance()
        {
            return FirebaseDBManager.Instance;
        }

        public void CheckUserExist(string uid, Action<bool> callback)
        {
            Debug.Log("#CheckUserExist " + uid);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("users/");
            reference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#CheckUserExist FAIL");
                    callback(false);
                    Debug.Log(task.Exception);
                    //return false;
                }
                else
                {
                    callback(task.Result.HasChild(uid));
                    //return true;
                }
            });
        }

        public virtual void SaveUserToServer(FirebaseAuthManager.UserDataInDatabase userData)
        {
            Debug.Log("#SaveUserToServer_");
            string s = JsonUtility.ToJson(userData);
            //string s = JsonConvert.SerializeObject(userData);
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("users/" + userData.uid);
            reference.SetRawJsonValueAsync(s).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#SaveUserToServer FAIL");
                    Debug.Log(task.Exception);
                }
            });
            Debug.Log("Server: OnSaveUserToServer");
            //print("OnSaveUserDate url : " + url);
        }

        public virtual void LoadUserData(string uid, Action<string, string, string> callback)
        {
            Debug.Log("#LoadUserData");
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("users/" + uid);
            reference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("#LoadUserData FAIL");
                    callback("", "", "");
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    FirebaseAuthManager.UserDataInDatabase userData = JsonUtility.FromJson<FirebaseAuthManager.UserDataInDatabase>(task.Result.GetRawJsonValue());
                    if (userData == null)
                    {
                        callback("", "", "");
                    }
                    else
                    {
                        Debug.Log("# LoadUserData username: " + userData.username);
                        callback?.Invoke(userData.username, userData.email, userData.uid);
                    }
                }
            });
            Debug.Log("Server: OnUserLogin");
            //print("OnUserLogin url : " + url);
        }
        public virtual void UpdateUsername(string username, System.Action<bool> callback=null) {
            Dictionary<string, System.Object> childUpdates = new Dictionary<string, System.Object>();
            childUpdates["username"] = username;

            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("users/" + _uid);
            reference.UpdateChildrenAsync(childUpdates).ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log("#UpdateUsername FAIL " + task.Exception);
                    callback?.Invoke(false);
                }
                callback?.Invoke(true);
                Debug.Log("Server: UpdateUsername done!");
            });
            Debug.Log("Server: UpdateUsername " + username);
        }
    }
}
