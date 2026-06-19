using System;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoardingMain: MonoBehaviour
    {
        public OnBoardingManager.steps step;
        [SerializeField] GameObject panel;
        void Awake()
        {
            Events.OnBoarding += OnBoarding;
            Hide();
        }
        void OnDestroy()
        {
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
            else
                Hide();
        }
        void Show()
        {
            if(panel == null)
                gameObject.SetActive(true);
            else
                panel.SetActive(true);
            OnShow();
        }
         void Hide()
        {
            if(panel == null)
                gameObject.SetActive(false);
            else
                panel.SetActive(false);
            OnHide();
        }
    }
}
