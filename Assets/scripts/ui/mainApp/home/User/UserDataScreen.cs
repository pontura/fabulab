using UnityEngine;
using Yaguar.Auth;

namespace UI.MainApp.Home.User
{
    public class UserDataScreen : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_InputField TMP_InputField;

        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn)
            {
                string username = Data.Instance.userData.userDataInDatabase.username;
                TMP_InputField.text = username;
            }
        }
        public void EditName()
        {
            string name = TMP_InputField.text;
            if (string.IsNullOrEmpty(name))
                return;
            ChangeName(name);
        }
        void ChangeName(string name)
        {
            Events.ChangeName(name);
        }
        public void Logout()
        {
            FirebaseAuthManager.Instance.SignOut();
        }
    }
}