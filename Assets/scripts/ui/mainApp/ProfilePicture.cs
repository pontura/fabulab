using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp
{
    public class ProfilePicture : MonoBehaviour
    {
        [SerializeField] Image image;
        public void InitOwner()
        {
            Data.Instance.cacheData.GetUser(Data.Instance.userData.userDataInDatabase.uid, OnReady);
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
