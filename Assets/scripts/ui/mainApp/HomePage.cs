using System;
using UnityEngine;

namespace UI.MainApp.Home
{
    public class HomePage : MainScreen
    {
        [SerializeField] HomeScreen home;
        [SerializeField] UserScreen user;

        public screens screen;
        public enum screens
        {
            home,
            user
        }

        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Home:
                    screen = screens.home;
                    Show(true);
                    home.Show(true);
                    user.Show(false);
                    break;
                case UIManager.screenType.UserScreen:
                    screen = screens.user;
                    Show(true);
                    home.Show(false);
                    user.Show(true);
                    break;
                default:
                    Show(false);
                    break;
            }
        }
    }
}
