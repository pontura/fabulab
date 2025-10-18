using Common.UI;
using System.Collections;
using UnityEngine;

namespace BoardItems.UI
{
    public class EditionTools : MonoBehaviour
    {
        [SerializeField] Transform container;
        [SerializeField] ItemsUI items;
        [SerializeField] TabController tabs;

        private void Awake()
        {
            Events.EditMode += EditMode;
            tabs.Init(OnTabClicked);
        }
        void OnTabClicked(int id)
        {
            switch(id)
            {
                case 0:
                    items.SetOn(false);
                    break;
                case 1:
                    items.SetOn(false); 
                    break;
                case 2:
                    items.SetOn(true);
                    break;
            }
        }
        private void OnDestroy()
        {
            Events.EditMode -= EditMode;
        }
        void EditMode(bool isOn)
        {
            gameObject.SetActive(isOn);
        }
    }
}
