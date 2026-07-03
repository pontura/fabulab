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
            Events.OnProfilePictureUpdated += OnProfilePictureUpdated;
        }

        private void OnDestroy() {
            FirebaseAuthManager.Instance.OnFirebaseAuthenticated -= OnFirebaseAuthenticated;
            Events.OnProfilePictureUpdated -= OnProfilePictureUpdated;
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
        private void OnReady(CacheData.UserData data, Texture2D tex)
        {
            OnProfilePictureUpdated(tex);
        }

        void OnProfilePictureUpdated(Texture2D tex) {
            image.sprite = tex != null ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)) : null;
        }
    }
}
