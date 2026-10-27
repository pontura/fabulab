using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class LightBtn : MonoBehaviour
    {
        [SerializeField] GameObject sliderPanel;
        [SerializeField] Slider slider;
        [SerializeField] Transform btnContainer;
        [SerializeField] SimpleButton btn_prefab;

        bool opened;
        float sliderValue;

        private void Start() {
            SetButtons();
        }

        public void Init(float sliderValue)
        {
            opened = false;
            sliderPanel.SetActive(false);
        }

        void SetButtons() {
            foreach (BgLigthtingPalette item in Scenario.Instance.sceneObjectsManager.BackgroundLighting.LightingPalettes) {
                Debug.Log("%% " + item.Id);
                SimpleButton btn = Instantiate(btn_prefab, btnContainer);
                btn.Init(item.Icon);
                btn.GetComponent<Button>().onClick.AddListener(() => BackgroundLightingPalette(item.Id,item.DefaultStep));
            }
        }

        void BackgroundLightingPalette(string id, int defaultStep) {
            ScenesManager.Instance.GetActiveScene().lightingId = id;
            ScenesManager.Instance.GetActiveScene().lightingValue = defaultStep;
            ResetSlider();
            OnSliderChanged();
        }

        public void Clicked()
        {
            if (opened) return;
            opened = true;
            sliderPanel.SetActive(true);
            ResetSlider();
        }

        void ResetSlider() {
            slider.maxValue = Scenario.Instance.sceneObjectsManager.BackgroundLighting.GetMaxValue();
            sliderValue = ScenesManager.Instance.GetActiveScene().lightingValue;
            slider.value = sliderValue;
        }
        public void SetValue(float sliderValue)
        {
            this.sliderValue = sliderValue;
        }
        public void OnSliderChanged()
        {
            if (!opened) return;
            ScenesManager.Instance.GetActiveScene().lightingValue = (int)slider.value;
            ScenesManager.Instance.MarkChanged();
            StoryMakerEvents.SetBackgroundLights();
            SetValue(slider.value);
        }
        public void Close()
        {
            opened = false;
            sliderPanel.SetActive(false);
        }
    }
}
