using Common.UI;
using System.Collections.Generic;
using UI.MainApp.Home.User;
using UnityEngine;

namespace UI.MainApp.Home
{
    public class UserScreen : MonoBehaviour
    {
        [SerializeField] TabController tabs;
        [SerializeField] UserDataScreen userDataScreen;
        [SerializeField] GameObject stories;
        [SerializeField] UserCharactersScreen charactersScreen;
        [SerializeField] UserObjectsScreen objects;

        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn)
            {
                tabs.Init(OnTabClicked);
                int charactersQty = Data.Instance.charactersData.userCharacters.Count;
                List<string> tabNames = new List<string>() { "Settings", "Stories", "Characters (" + charactersQty + ")", "Objects" };
                tabs.SetTabNames(tabNames);
            }
        }
        void OnTabClicked(int id)
        {
            print("OnTabClicked " + id + " name: "  +gameObject.name);

            userDataScreen.Show(false);
            charactersScreen.Show(false);

            stories.SetActive(false);
            objects.Show(false);

            switch (id)
            {
                case 0:
                    userDataScreen.Show(true);
                    break;
                case 1:
                    stories.SetActive(true);
                    break;
                case 2:
                    charactersScreen.Show(true);
                    break;
                case 3:
                    objects.Show(true);
                    break;
            }
        }
    }
}
