using UnityEngine;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class LightBtn : MonoBehaviour
    {
        [SerializeField] GameObject sliderPanel;
        [SerializeField] Slider slider;

        bool opened;
        float sliderValue;

        public void Init(float sliderValue)
        {
            opened = false;
            sliderPanel.SetActive(false);
        }
        public void Clicked()
        {
            if (opened) return;
            opened = true;
            sliderPanel.SetActive(true);
            slider.value = sliderValue;
        }
        public void SetValue(float sliderValue)
        {
            this.sliderValue = sliderValue;
        }
        public void OnSliderChanged()
        {
            if (!opened) return;
            StoryMakerEvents.SetBackgroundLights(slider.value);
            SetValue(slider.value);
        }
        public void Close()
        {
            opened = false;
            sliderPanel.SetActive(false);
        }
    }
}
