using System;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp
{
    public class GhostImage : MonoBehaviour
    {
        [SerializeField] Image image;

        void Start()
        {
            StoryMakerEvents.OnTimelinePlay += OnTimelinePlay;
        }
        void OnDestroy()
        {
            StoryMakerEvents.OnTimelinePlay -= OnTimelinePlay;
        }
        public void Reset()
        {
            image.sprite = null;          
        }
        private void OnTimelinePlay(bool isPlay)
        {
            if(image.sprite == null) 
                image.enabled = false;
            else
                image.enabled = !isPlay;
        }
        public void Show(bool isOn)
        {  
            if(image.sprite == null) 
                image.enabled = false;
            else
                image.enabled = isOn;            
        }
        public void Init(Sprite s)
        {
            image.enabled = s != null;
            if(s != null)
                image.sprite = s;
        }
    }
}
