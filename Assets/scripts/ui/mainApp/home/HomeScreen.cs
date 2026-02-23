using Common.UI;
using System.Collections.Generic;
using UI.MainApp.Home.User;
using UnityEngine;

namespace UI.MainApp.Home
{
    public class HomeScreen : MonoBehaviour
    {
        [SerializeField] TabController tabs;
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
                List<string> tabNames = new List<string>() { "Stories", "Characters (" + charactersQty + ")", "Objects" };
                tabs.SetTabNames(tabNames);
            }
        }
        void OnTabClicked(int id)
        {
            print("On home TabClicked " + id);

            charactersScreen.Show(false);
            objects.Show(false);

            stories.SetActive(false);

            switch (id)
            {
                case 0:
                    stories.SetActive(true);
                    break;
                case 1:
                    charactersScreen.Show(true);
                    break;
                case 2:
                    objects.Show(true);
                    break;
            }
        }
    }
}
