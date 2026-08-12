using System;
using System.Globalization;
using UI;
using UI.MainApp;
using UI.MainApp.Home.User;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace OnBoarding
{
    public class OnBoarding_StoryPresentationDone : OnBoarding_GenericText
    {
        [SerializeField] GameObject part;
        
        [SerializeField] PlayButton playButton;

        public override void OnShow()
        {            
            part.SetActive(false);
            field.text = "¡Ahora mirá las historias que se pueden hacer en Fabulab!";   
            playButton.OnClick();   

            StoryMakerEvents.OnMovieOver += OnMovieOver;
        }

        private void OnBoardingStepDone(OnBoardingManager.steps steps)
        {
            if(active && steps != step) return;            
            part.SetActive(true);
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
        }

        private void OnMovieOver() {
            part.SetActive(true);
            Invoke(nameof(ClearScene), Time.deltaTime * 2);
        }

        void ClearScene() {
            StoryMakerEvents.SetEditing(false);
            StoryMakerEvents.ClearScene();
        }

        void OnDestroy()
        {            
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
        }
        public void OnDone()
        {
            Done();
            UIManager.Instance.Home();
        }
        public override void ShowPanelsBack() {}
    }
}
