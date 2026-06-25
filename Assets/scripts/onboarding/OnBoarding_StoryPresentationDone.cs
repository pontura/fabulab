using System;
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
            field.text = "Ahora probá modificando lo que quieras";   
            playButton.OnClick();     

            StoryMakerEvents.OnMovieOver += OnMovieOver;
        }

        private void OnBoardingStepDone(OnBoardingManager.steps steps)
        {
            if(active && steps != step) return;            
            part.SetActive(true);
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
        }

        private void OnMovieOver()
        {
            part.SetActive(true);
        }

        void OnDestroy()
        {            
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
        }
        public override void ShowPanelsBack() {}
    }
}
