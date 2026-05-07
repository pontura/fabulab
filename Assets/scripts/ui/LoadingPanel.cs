using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{

    public class LoadingPanel : MonoBehaviour
    {
        public GameObject panel;
        Animation anim;

        void Start()
        {
            Events.OnLoading += OnLoading;
            Events.OnLoadingParent += OnLoadingParent;
            //OnLoading(false);
        }
        void OnDestroy()
        {
            Events.OnLoading -= OnLoading;
            Events.OnLoadingParent -= OnLoadingParent;
        }
        void OnLoadingParent(Transform parent)
        {
            if (parent == null)
                parent = UIManager.Instance.transform;

            panel.transform.SetParent(parent);

            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;

            rt.offsetMin = Vector2.zero; 
            rt.offsetMax = Vector2.zero; 

            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
        }
        void OnLoading(bool isOn)
        {
            print("OnLoading " + isOn);
            if (!isOn)
            {
                panel.GetComponent<Animation>().Play("loading_home_out");
                Invoke("SetOff", 1);
            } else {
                panel.SetActive(isOn);
                panel.GetComponent<Animation>().Play("loading_home");
            }                
        }
        void SetOff()
        {
            panel.SetActive(false);
        }
    }

}