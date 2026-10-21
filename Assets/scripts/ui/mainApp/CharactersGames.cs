using Newtonsoft.Json.Converters;
using UI.MainApp.Home.User;
using UnityEngine;

namespace UI.MainApp
{
    public class CharactersGames : MonoBehaviour
    {
        [SerializeField] CharactersGameScreen charactersGameScreen;
        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            charactersGameScreen.Show(true);
        }
    }
}
