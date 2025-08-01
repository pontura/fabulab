using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BoardItems.UI
{
    public class LoadingPanel : MonoBehaviour
    {
        public GameObject panel;
        Animation anim;

        void Start()
        {
            Events.OnLoading += OnLoading;
            OnLoading(false);
        }
        void OnDestroy()
        {
            Events.OnLoading -= OnLoading;
        }
        void OnLoading(bool isOn)
        {
            print("OnLoading " + isOn);
            if (!isOn)
            {
                panel.GetComponent<Animation>().Play("loading_out");
                Invoke("SetOff", 1);
            }
            else
                panel.SetActive(isOn);
        }
        void SetOff()
        {
            panel.SetActive(false);
        }
    }

}