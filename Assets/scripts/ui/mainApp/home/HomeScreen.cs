using UnityEngine;

namespace UI
{
    public class HomeScreen : MonoBehaviour
    {
        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
        }
    }
}
