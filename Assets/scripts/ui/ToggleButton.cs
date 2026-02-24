using UnityEngine;

namespace UI
{
    public class ToggleButton : MonoBehaviour
    {
        [SerializeField] GameObject on;
        [SerializeField] GameObject off;
        bool isOn;
        System.Action<bool> OnToggle;

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }
        public void Init(System.Action<bool> OnToggle, bool isOn = false)
        {
            this.OnToggle = OnToggle;
            this.isOn = isOn;
            on.SetActive(this.isOn);
            off.SetActive(!this.isOn);
        }
        public void Toggle()
        {
            this.isOn = !isOn;
            Set();
        }
        void Set()
        {
            on.SetActive(this.isOn);
            off.SetActive(!this.isOn);
            OnToggle(this.isOn);
        }
    }
}
