using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class PlayButton : MonoBehaviour
    {
        [SerializeField] GameObject on;
        [SerializeField] GameObject off;
        [SerializeField] FilmMakerManager filmMakerUI;
        bool isOn;        

        private void Start()
        {
            isOn = false;
            SetButtons(isOn);
            StoryMakerEvents.OnTimelinePlay(isOn);
        }
        public void OnClick()
        {
            isOn = !isOn;
            SetButtons(isOn);
            StoryMakerEvents.OnTimelinePlay(isOn);
        }
        public void SetButtons(bool isOn)
        {
            this.isOn = isOn;
            if (isOn)
            {
                on.SetActive(true);
                off.SetActive(false);
            }
            else
            {
                on.SetActive(false);
                off.SetActive(true);
            }
        }
    }
}
