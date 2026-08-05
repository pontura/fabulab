using UnityEngine;

namespace UI
{
    public class ToggleButton : MonoBehaviour
    {
        [SerializeField] protected GameObject on;
        [SerializeField] protected GameObject off;
        protected bool isOn;
        protected System.Action<bool> OnToggle;
        System.Action<int> OnButtonClicked;
        public int id;
        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }
        public void InitButton(int id, System.Action<int> OnButtonClicked)
        {
            this.id = id;
            this.OnButtonClicked = OnButtonClicked;
        }
         public void OnClicked()
        {
            OnButtonClicked(id);
        }
        public void Init(System.Action<bool> OnToggle, bool isOn = false)
        {
            this.OnToggle = OnToggle;
            this.isOn = isOn;
            on.SetActive(this.isOn);
            off.SetActive(!this.isOn);
        }
        public void Clicked()
        {
            OnToggle(this.isOn);
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
        public void Force(bool isOn)
        {
            this.isOn = isOn;
            on.SetActive(this.isOn);
            off.SetActive(!this.isOn);
        }
    }
}
