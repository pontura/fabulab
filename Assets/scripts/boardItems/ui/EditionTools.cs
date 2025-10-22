using Common.UI;
using UnityEngine;

namespace BoardItems.UI
{
    public class EditionTools : MonoBehaviour
    {
        [SerializeField] ItemsUI items;
        [SerializeField] ActionsUI actions;
        [SerializeField] EmojisUI emojis;
        [SerializeField] TabController tabs;

        private void Awake()
        {
            tabs.Init(OnTabClicked);
        }
        private void Start()
        {
            OnTabClicked(0);
        }
        void OnTabClicked(int id)
        {
            switch(id)
            {
                case 0:
                    items.SetOn(true);
                    actions.SetOn(false);
                    emojis.SetOn(false);
                    break;
                case 1:
                    items.SetOn(false);
                    actions.SetOn(true);
                    emojis.SetOn(false);
                    break;
                case 2:
                    items.SetOn(false);
                    actions.SetOn(false);
                    emojis.SetOn(true);
                    break;
            }
        }
    }
}
