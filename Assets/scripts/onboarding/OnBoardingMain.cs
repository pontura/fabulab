using System;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoardingMain: MonoBehaviour
    {
        public OnBoardingManager.steps step;
        [SerializeField] GameObject panel;
        [SerializeField] GameObject[] hideOnActive;

        public bool active;
        public void Init()
        {
            Events.OnBoarding += OnBoarding;
            Reset();
        }
        void OnDestroy()
        {
            print("OnDestroy");
            Events.OnBoarding -= OnBoarding;
        }
        public void Done()
        {            
            Events.OnBoardingDone(step);
            Hide();
        }

         public virtual void OnShow(){}
         public virtual void OnHide() {}
        private void OnBoarding(OnBoardingManager.steps step)
        {
           if(step == this.step)
                Show();
            else if(active)
                Hide();
        }
        void Show()
        {
            active = true;
            if(hideOnActive.Length>0)
            {
                foreach(GameObject go in hideOnActive)
                    go.SetActive(false);
            }
            if(panel == null)
                gameObject.SetActive(true);
            else
                panel.SetActive(true);
            OnShow();
        }
        public void Reset()
        {
             if(panel == null)
                gameObject.SetActive(false);
            else
                panel.SetActive(false);
        }
        public virtual void ShowPanelsBack()
        {
            print("ShowPanelsBack " + this.gameObject.name);
             foreach(GameObject go in hideOnActive)
                    go.SetActive(true);
        }
         void Hide()
        {
            active = false;
            if(hideOnActive.Length>0)
                ShowPanelsBack();
            Reset();
            OnHide();
        }
    }
}
