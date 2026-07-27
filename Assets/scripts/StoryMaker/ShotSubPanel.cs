using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class ShotSubPanel : MonoBehaviour
    {
        public GameObject duration;
        public CamManagerUI cameras;

        public enum types
        {
            duration,
            cameras
        }
        public void Init(types type)
        {
            switch(type)
            {
                case types.duration:
                duration.gameObject.SetActive(true);
                break;
                case types.cameras:
                cameras.gameObject.SetActive(true);
                break;
            }
        }
        public void Reset()
        {
            duration.SetActive(false);
            cameras.gameObject.SetActive(false);            
        }
    }
}
