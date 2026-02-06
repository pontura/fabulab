using Common.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.MainApp.Home
{
    public class UserScreen : MonoBehaviour
    {
        [SerializeField] TabController tabs;
        [SerializeField] AlbumUI album;
        [SerializeField] GameObject stories;
        [SerializeField] GameObject objects;
        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn)
            {
                tabs.Init(OnTabClicked, 1);
                album.Init();
            }
        }
        void OnTabClicked(int id)
        {
            print("OnTabClicked " + id);
            stories.SetActive(false);
            album.gameObject.SetActive(false);
            objects.SetActive(false);

            switch (id)
            {
                case 0:
                    stories.SetActive(true);
                    break;
                case 1:
                    album.gameObject.SetActive(true);
                    break;
                case 2:
                    objects.SetActive(true);
                    break;
            }
        }
    }
}
