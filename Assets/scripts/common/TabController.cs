using System.Collections.Generic;
using UnityEngine;

namespace Common.UI
{
    public class TabController : MonoBehaviour
    {
       // [SerializeField] TabButton tabButton;
        [SerializeField] List<TabButton> all;

        void Start()
        {
            foreach (TabButton tabButton in all)
                tabButton.Init(this);
        }
        public void Clicked(TabButton tabButton)
        {
            ResetAll();
            tabButton.SetActive();
            print("clicked " + tabButton);
        }
        void ResetAll()
        {
            foreach (TabButton tabButton in all)
                tabButton.Reset();
        }
    }
}
