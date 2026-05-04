using UnityEngine;

namespace UI
{
    public enum LoadingType
    {
        Fullscreen,
        Home
    }

    public class LoadingPanel : MonoBehaviour
    {
        public GameObject panel;
        Animation anim;

        void Start()
        {
            Events.OnLoading += OnLoading;
            //OnLoading(false);
        }
        void OnDestroy()
        {
            Events.OnLoading -= OnLoading;
        }
        void OnLoading(bool isOn, LoadingType loadingType)
        {
            print("OnLoading " + isOn);
            if (!isOn)
            {
                string animName = loadingType == LoadingType.Home ? "loading_home_out" : "loading_fullscreen_out";
                panel.GetComponent<Animation>().Play(animName);
                Invoke("SetOff", 1);
            } else {
                panel.SetActive(isOn);
                string animName = loadingType == LoadingType.Home ? "loading_home" : "loading_fullscreen";
                panel.GetComponent<Animation>().Play(animName);
            }                
        }
        void SetOff()
        {
            panel.SetActive(false);
        }
    }

}