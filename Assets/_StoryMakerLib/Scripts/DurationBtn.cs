using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class DurationBtn : MonoBehaviour
    {
        [SerializeField] Image bar;
        Timeline timeline;
        [SerializeField] GameObject sliderPanel;
        [SerializeField] Slider slider;
        [SerializeField] TMPro.TMP_Text sliderField;
        [SerializeField] float sliderFactor;
        float duration;
        bool opened;
        float sliderValue;


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
            sliderPanel.SetActive(true);
            slider.value = bar.fillAmount / sliderFactor;
            SetText();
        }
        public void SetValue(float sliderValue)
        {
            this.sliderValue = sliderValue;
            if (sliderValue < 0.05f)
                bar.fillAmount = 0.05f;
            else
                bar.fillAmount = sliderValue;
        }
        public void OnSliderChanged()
        {
            if (!opened) return;
            duration = timeline.OnChangeDuration(slider.value * sliderFactor);
            SetText();
            SetValue(slider.value * sliderFactor);
        }
        public void Close()
        {
            opened = false;
            sliderPanel.SetActive(false);
        }
        public void SetText()
        {
            int seconds = (int)duration;
            int milliseconds = (int)((duration - seconds) * 1000);

            sliderField.text = $"{seconds}s {milliseconds}ms";
        }
    }
}
