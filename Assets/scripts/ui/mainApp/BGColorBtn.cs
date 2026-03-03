using UnityEngine;

namespace UI.MainApp
{
    public class BGColorBtn : MonoBehaviour
    {
        public void SetOn(bool isOn)
        {
            gameObject.SetActive(isOn);
        }
        public void OnClicked()
        {
            Events.OnBGColorizerOpen(true);
        }
    }
}
