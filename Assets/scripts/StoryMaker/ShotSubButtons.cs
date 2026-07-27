using UnityEngine;
namespace Yaguar.StoryMaker.Editor
{
    public class ShotSubButtons : MonoBehaviour
    {
        public GameObject durations;
        public CamManagerUI camManagerUI;
        public ShotButtons shotButtons;

        void Awake()
        {            
            camManagerUI.Init();
            Reset();
        }
        public enum types
        {
            duration,
            camera
        }
        public void Init(types type)
        {
            shotButtons.Open();
            Reset();

            switch(type)
            {
                case types.camera:
                camManagerUI.gameObject.SetActive(true);
                camManagerUI.OpenCam();
                break;
                case types.duration:
                durations.gameObject.SetActive(true);
                break;
            }
        }
        public void Close()
        {
            shotButtons.Close();
        }
        public void Reset()
        {
            durations.gameObject.SetActive(false);
            camManagerUI.gameObject.SetActive(false);
        }
    }
}
