using UnityEngine;
using UnityEngine.UI;
using static BoardItems.AlbumData;

namespace BoardItems.UI
{
    public class PresetButton : MonoBehaviour
    {
        [SerializeField] Image image;
        PresetsSelector manager;
        public WorkData workData;
        public void Init(PresetsSelector manager, WorkData workData)
        {
            this.workData = workData;
            this.manager = manager;
            image.sprite = workData.GetSprite();
        }
        public void OnClick()
        {
            manager.OnClicked(this);
        }
    }

}