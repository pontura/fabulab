using UnityEngine;
using UnityEngine.UI;
using Yaguar.Auth;

namespace UI.MainApp
{
    public class ProfilePicture : MonoBehaviour
    {
        [SerializeField] Image image;

        private void Start() {
            FirebaseAuthManager.Instance.OnFirebaseAuthenticated += OnFirebaseAuthenticated;
        }

        private void OnDestroy() {
            FirebaseAuthManager.Instance.OnFirebaseAuthenticated -= OnFirebaseAuthenticated;
        }

        void OnFirebaseAuthenticated(string username, string email, string uid) {
            if (uid != "" && uid != null)
                Data.Instance.cacheData.GetUser(uid, OnReady);
        }

        public void InitOwner()
        {            
            string uid = Data.Instance.userData.userDataInDatabase.uid;
            if (uid != "" && uid != null)
                Data.Instance.cacheData.GetUser(uid, OnReady);
        }
        public void Init(string uid)
        {
            Data.Instance.cacheData.GetUser(uid, OnReady);
        }
        private void OnReady(CacheData.UserData data)
        {
            image.sprite = data.thumb != null ? Sprite.Create(data.thumb, new Rect(0, 0, data.thumb.width, data.thumb.height), new Vector2(0.5f, 0.5f)) : null;
        }
    }
}
