using System;
using UnityEngine;

namespace UI.MainApp.Home
{
    public class Header : MonoBehaviour
    {
        [SerializeField] GameObject editBtn;
        [SerializeField] TMPro.TMP_Text usernameField;

        private void Awake()
        {
            Events.ChangeName += OnChangeName;
        }
        private void OnDestroy()
        {
            Events.ChangeName -= OnChangeName;
        }

        private void OnChangeName(string username)
        {
            usernameField.text = username;
        }

        public void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Home:
                    string username = Data.Instance.userData.userDataInDatabase.username;
                    OnChangeName(username);
                    editBtn.SetActive(true);
                    break;
                case UIManager.screenType.UserScreen:
                    editBtn.SetActive(false);
                    break;
            }
        }
        public void Edit()
        {
            Events.ShowScreen(UIManager.screenType.UserScreen);
        }
        public void Create()
        {
            UIManager.Instance.Create();
        }
    }
}
