using UnityEngine;

namespace UI.MainApp.Home
{
    public class HomeScreen : MonoBehaviour
    {
        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
        }
    }
}
