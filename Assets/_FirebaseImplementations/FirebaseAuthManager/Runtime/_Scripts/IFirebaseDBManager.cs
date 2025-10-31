using System;
using Yaguar.Auth;

namespace Yaguar.DB
{
    public interface IFirebaseDBManager
    {

        void SaveUserToServer(FirebaseAuthManager.UserDataInDatabase userData);

        void LoadUserData(string uid, Action<string, string, string> callback);

        void CheckUserExist(string uid, Action<bool> callback);

        IFirebaseDBManager GetInstance();
    } 
}
