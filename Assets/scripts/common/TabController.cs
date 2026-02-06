using System.Collections.Generic;
using UnityEngine;

namespace Common.UI
{
    public class TabController : MonoBehaviour
    {
       // [SerializeField] TabButton tabButton;
        [SerializeField] List<TabButton> all;
        [SerializeField] Sprite[] tab_images;
        public List<TabButton> All
        {
            get { return all; }
        }
        System.Action<int> OnActive;
        void Start()
        {
            int id = 0;
            foreach (TabButton tabButton in all)
            {
                tabButton.Init(this, id);
                if(tab_images.Length>id)
                    tabButton.SetThumb(tab_images[id]);
                id++;
            }
        }
        public void SetTabNames(List<string> names)
        {
            int id = 0;
            foreach (TabButton tabButton in all)
            {
                tabButton.SetName(names[id]);
                id++;
            }
        }
        public void Init(System.Action<int> OnActive, int initialID = 0)
        {
            this.OnActive = OnActive;
            Clicked(all[initialID]);
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
