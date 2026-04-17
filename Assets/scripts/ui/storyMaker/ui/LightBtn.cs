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
            foreach (BgLigthtingPalette item in (Scenario.Instance.sceneObejctsManager as SceneObjectsManagerFabulab).BackgroundLighting.LightingPalettes) {
                Debug.Log("%% " + item.Id);
                SimpleButton btn = Instantiate(btn_prefab, btnContainer);
                btn.Init(item.Icon);
                btn.GetComponent<Button>().onClick.AddListener(() => BackgroundLightingPalette(item.Id));
            }
        }

        void BackgroundLightingPalette(string id) {
            ScenesManagerFabulab.Instance.GetActiveScene().lightingId = id;
            ScenesManagerFabulab.Instance.GetActiveScene().lightingValue = 0;
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
            slider.maxValue = (Scenario.Instance.sceneObejctsManager as SceneObjectsManagerFabulab).BackgroundLighting.GetMaxValue();
            sliderValue = ScenesManagerFabulab.Instance.GetActiveScene().lightingValue;
            slider.value = sliderValue;
        }
        public void SetValue(float sliderValue)
        {
            this.sliderValue = sliderValue;
        }
        public void OnSliderChanged()
        {
            if (!opened) return;
            ScenesManagerFabulab.Instance.GetActiveScene().lightingValue = (int)slider.value;
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
