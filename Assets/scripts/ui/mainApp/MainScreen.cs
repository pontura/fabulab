using System;
using UnityEngine;

namespace UI.MainApp
{
    public class MainScreen : MonoBehaviour
    {
        [SerializeField] GameObject panel;


        private void Awake()
        {
            Events.ShowScreen += ShowScreen;
        }
        private void OnDestroy()
        {
            Events.ShowScreen -= ShowScreen;
            OnDestroyed();
        }

        public  virtual void OnDestroyed() { }
        protected virtual void ShowScreen(UIManager.screenType type) { }

        public void Show(bool isOn)
        {
            panel.SetActive(isOn);
            if (isOn) OnInit();
        }
        public virtual void OnInit() { }
    }
}
