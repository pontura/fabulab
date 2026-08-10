using UnityEngine;

namespace UI
{
    public class LikeToggle : ToggleButton
    {
        [SerializeField] TMPro.TextMeshProUGUI countField;

        public void Init(System.Action<bool> OnToggle, int likeCount, bool isOn = false) {
            this.OnToggle = OnToggle;
            this.isOn = isOn;
            on.SetActive(this.isOn);
            off.SetActive(!this.isOn);
            countField.text = likeCount<=0?"":""+ likeCount;
            //Debug.Log("% ACA: "+countField.text);
        }
    }
}
