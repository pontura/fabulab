using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace OnBoarding
{
    public class OnBoarding_Movie : OnBoardingMain
    {
        [SerializeField] GameObject loading;
        [SerializeField] VideoPlayer videoPlayer;
        [SerializeField] string url;
        public TMPro.TMP_Text field;
        Action OnXtraStepDone;

        public override void OnBoardingXtraStep(OnBoardingManager.steps step, Action OnXtraStepDone)
        {
            loading.SetActive(true);
            this.OnXtraStepDone = OnXtraStepDone;
            base.OnBoardingXtraStep(step, OnXtraStepDone);
            OnBoarding(step);
        }        
        public override void OnShow()
        {
            videoPlayer.Stop();
            switch(step)
            {
                case OnBoardingManager.steps.video_character:
                    field.text = "Así se hacen los personajes.";
                break;
                case OnBoardingManager.steps.video_object:
                    field.text = "Así se hace un objeto.";
                break;
                  case OnBoardingManager.steps.video_story:
                    field.text = "Así se hacen las historias.";
                break;
                  case OnBoardingManager.steps.video_bg:
                    field.text = "Así se hace un escenario.";
                break;
            }       
            videoPlayer.url = url;
            videoPlayer.playOnAwake = true;
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnVideoReady;
            videoPlayer.loopPointReached += OnVideoFinished;    
        }
        void OnVideoReady(VideoPlayer vp)
        {
            loading.SetActive(false);
            videoPlayer.prepareCompleted -= OnVideoReady;
            videoPlayer.Play();
        }
        void OnVideoFinished(VideoPlayer vp)
        {
            OnVideoFinished();
        }
        public void OnVideoFinished()
        {             
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.Stop();
            OnXtraStepDone();
            Hide();
        }
    }
}
