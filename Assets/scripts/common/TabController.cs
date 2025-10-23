using System.Collections.Generic;
using UnityEngine;

namespace Common.UI
{
    public class TabController : MonoBehaviour
    {
       // [SerializeField] TabButton tabButton;
        [SerializeField] List<TabButton> all;
        System.Action<int> OnActive;
        void Start()
        {
            int id = 0;
            foreach (TabButton tabButton in all)
            {
                tabButton.Init(this, id);
                id++;
            }
        }
        public void Init(System.Action<int> OnActive)
        {
            this.OnActive = OnActive;
        }
        public void Clicked(TabButton tabButton)
        {
            ResetAll();
            tabButton.SetActive();
            OnActive(tabButton.id);
        }
        void ResetAll()
        {
            foreach (TabButton tabButton in all)
                tabButton.Reset();
        }
    }
}
