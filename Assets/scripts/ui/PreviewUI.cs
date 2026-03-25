using BoardItems.Characters;
using BoardItems.UI;
using Common.UI;
using UnityEngine;

namespace UI
{
    public class PreviewUI : MonoBehaviour
    {
        [SerializeField] ActionsUI actions;
        [SerializeField] EmojisUI emojis;
        [SerializeField] TabController tabs;

        public void SetOff()
        {
            gameObject.SetActive(false);
        }
        public void Init()
        {
            Events.OnBodyPartActive(CharacterPartsHelper.parts.none);
            Events.Zoom(0, false);
            gameObject.SetActive(true);
            tabs.Init(OnTabClicked); 
            Events.DragAndDropActive(false);
        }
        void OnTabClicked(int id)
        {
            switch (id)
            {
                case 0:
                    actions.SetOn(true);
                    emojis.SetOn(false);
                    break;
                case 1:
                    actions.SetOn(false);
                    emojis.SetOn(true);
                    break;
            }
        }
    }
}