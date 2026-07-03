using UnityEngine;
namespace Yaguar.StoryMaker.Editor
{
    public class ShotButtons : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] GameObject subPanel;
        [SerializeField] GameObject deleteBtn;
        [SerializeField] GameObject marker;
         [SerializeField] DurationBtn durationBtn;
        Animation anim;
        bool opened;
        public void SetFirstFrame(bool isFirstFrame)
        {
            deleteBtn.SetActive(!isFirstFrame);            
        }

        public void Show(bool isOn)
        {
            if(isOn)
            {
                int frame = ScenesManagerFabulab.Instance.currentSceneId;
                SetFirstFrame(frame == 1);
            }
            panel.SetActive(isOn);
        }
        public void OnMarkerUpdated(float timer_pos)
        {
            if(opened) return;
            if(anim == null)
                anim = GetComponent<Animation>();
            anim.Play();
            marker.transform.localPosition = new Vector3(timer_pos, marker.transform.localPosition.y, marker.transform.localPosition.z);
        }
        public void DurationBtnClicked()
        {
            Open();
            durationBtn.Clicked();
        }
        public void Open()
        {
            opened = true;
            if(anim == null)
                anim = GetComponent<Animation>();
            anim.Play("open");
            
        }
        public void Close()
        {
            opened = false;
            durationBtn.Close();
            if(anim == null)
                anim = GetComponent<Animation>();
            anim.Play("close");
            
        }

    }
}
