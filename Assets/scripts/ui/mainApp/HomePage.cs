using System;
using UnityEngine;

namespace UI.MainApp.Home
{
    public class HomePage : MainScreen
    {
        [SerializeField] HomeScreen home;
       // [SerializeField] UserScreen user;

        public screens screen;
        public enum screens
        {
            stories,
            characters,
            objects,
            user
        }
        void Start()
        {
            home.Init(this);
        }
        public void OnSelected(screens s)
        {
            screen = s;
        }
        protected override void ShowScreen(UIManager.screenType type)
        {
            switch (type)
            {
                case UIManager.screenType.Home:
                    Show(true);
                    screen = screens.stories;
                    home.Show(true);
                    home.OnTabSelect(screen);
                    break;
                case UIManager.screenType.UserScreen:
                    Show(true);
                    home.Show(true);
                    screen = screens.user;
                    home.OnTabSelect(screen);
                    break;
                default:
                    Show(false);
                    break;
            }
        }
    }
}
