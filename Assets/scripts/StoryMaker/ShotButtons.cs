using UnityEngine;
using UnityEngine.UI;
namespace Yaguar.StoryMaker.Editor
{
    public class ShotButtons : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Button addBtn;
        [SerializeField] Button deleteBtn;
        [SerializeField] Button camBtn;
        [SerializeField] Toggle moveBtn;
        [SerializeField] GameObject marker;
         [SerializeField] DurationBtn durationBtn;
        Animation anim;
        bool opened;
        public void SetFirstFrame(bool isFirstFrame)
        {
            deleteBtn.gameObject.SetActive(!isFirstFrame);            
        }
        
        public void Show(bool isOn)
        {
            if(isOn)
            {
                int frame = ScenesManagerFabulab.Instance.currentSceneId;
                SetFirstFrame(frame <2);
            }
            panel.SetActive(isOn);
        }
        public void OnMarkerUpdated(float timer_pos)
        {
            if(opened) return;
            int frame = ScenesManagerFabulab.Instance.currentSceneId;
                SetFirstFrame(frame <2);
            if(anim == null)
                anim = GetComponent<Animation>();
            anim.Play();
            marker.transform.localPosition = new Vector3(timer_pos, marker.transform.localPosition.y, marker.transform.localPosition.z);
        }
        public void DurationBtnClicked()
        {
            Open();
            durationBtn.Clicked();
            SetInteracions(false);
            durationBtn.GetComponent<Button>().interactable = true;
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
            SetInteracions(true);
            opened = false;
            durationBtn.Close();
            if(anim == null)
                anim = GetComponent<Animation>();
            anim.Play("close");
            
        }
        void SetInteracions(bool interactable)
        {
            addBtn.interactable = interactable;
            deleteBtn.interactable = interactable;
            camBtn.interactable = interactable;
            moveBtn.interactable = interactable;
            durationBtn.GetComponent<Button>().interactable = interactable;
        }

    }
}
