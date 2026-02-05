using System;
using UnityEngine;

namespace UI.MainApp.Home
{
    public class HomePage : MainScreen
    {
        [SerializeField] Header header;
        [SerializeField] HomeScreen home;
        [SerializeField] UserScreen user;

        protected override void ShowScreen(UIManager.screenType type)
        {
            header.ShowScreen(type);
            switch (type)
            {
                case UIManager.screenType.Home:
                    Show(true);
                    break;
                default:
                    Show(false);
                    break;
            }
        }
    }
}
