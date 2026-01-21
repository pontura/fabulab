using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class ObjectFinalSignal : Objeto
    {
        [SerializeField] GameObject panelToActive;
        [SerializeField] GameObject signal_to_hide;
        bool playingAnim;

        private void Start()
        {
            Reset();
            StoryMakerEvents.OnMovieOver += OnMovieOver;
            StoryMakerEvents.AddSceneObject += AddSceneObject;
            Loop();
        }
        void Loop()
        {
            if (!playingAnim)
            {
                if (FilmMakerManager.Instance.State == FilmMakerManager.states.PLAYING)
                    Reset();
                else
                    Paused();
            }

            Invoke("Loop", 0.1f);
        }
        private void OnDestroy()
        {
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
            StoryMakerEvents.AddSceneObject -= AddSceneObject;
        }
        void JumpTo(int framNum)
        {
            Reset();
        }
        void AddSceneObject(SOData s)
        {
            Paused();
        }
        void OnMovieOver()
        {
            PlayAnim();
        }
        void Paused()
        {
            playingAnim = false;
            panelToActive.SetActive(false);
            signal_to_hide.SetActive(true);
        }
        void PlayAnim()
        {
            playingAnim = true;
            panelToActive.SetActive(true);
            signal_to_hide.SetActive(false);
        }
        void Reset()
        {
            playingAnim = false;
            panelToActive.SetActive(false);
            signal_to_hide.SetActive(false);
        }
    }
}
