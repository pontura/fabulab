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
            Events.OnBoardingXtraStep += OnBoardingXtraStep;
            Reset();
        }
        void OnDestroy()
        {
            print("OnDestroy");
            Events.OnBoarding -= OnBoarding;
            Events.OnBoardingXtraStep -= OnBoardingXtraStep;
        }
        Action OnXtraStepDone;
        public virtual void OnBoardingXtraStep(OnBoardingManager.steps step, Action OnXtraStepDone)
        {
            this.OnXtraStepDone = OnXtraStepDone;
        }

        public void Done()
        {
            AudioManager.Instance.uiSfxManager.PlayNextScale("click", new int[] { 0, 2, 5 });
            Events.OnBoardingDone(step);
            Hide();
        }

        public virtual void OnShow(){}
        public virtual void OnHide() {}

        public void OnBoarding(OnBoardingManager.steps step)
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
        public void Hide()
        {
            active = false;
            if(hideOnActive.Length>0)
                ShowPanelsBack();
            Reset();
            OnHide();
        }
    }
}
