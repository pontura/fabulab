using System;
using UnityEngine;

namespace UI.MainApp
{
    public class MainScreen : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        UIManager.screenType lastType = UIManager.screenType.WorkDetail;

        private void Awake()
        {
            Events.ShowScreen += OnShowScreen;
        }
        private void OnDestroy()
        {
            Events.ShowScreen -= OnShowScreen;
            OnDestroyed();
        }

        public  virtual void OnDestroyed() { }

        void OnShowScreen(UIManager.screenType type) {
            if (type == lastType) return;
            lastType = type;
            ShowScreen(type);
        }
        protected virtual void ShowScreen(UIManager.screenType type) { }

        public void Show(bool isOn)
        {
            panel.SetActive(isOn);
            if (isOn) OnInit();
        }
        public virtual void OnInit() { }
    }
}
