using UnityEngine;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class DurationBtn : MonoBehaviour
    {
        Timeline timeline;
        [SerializeField] GameObject sliderPanel;
        [SerializeField] Slider slider;
        float duration;
        bool opened;
        public void Init(Timeline timeline, float duration)
        {
            opened = false;
            print("DurationBtn duration: " + duration);
            this.duration = duration;
            sliderPanel.SetActive(false);
            this.timeline = timeline;
        }
        public void Clicked()
        {
            if (opened) return;
            opened = true;
            slider.value = duration;
            sliderPanel.SetActive(true);
        }
        public void SetValue()
        {
            slider.value = duration;
        }
        public void OnSliderChanged()
        {
            timeline.OnChangeDuration(slider.value);
        }
        public void Close()
        {
            opened = false;
            sliderPanel.SetActive(false);
        }
    }
}
