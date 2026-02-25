using Common.UI;
using System.Collections.Generic;
using UnityEngine;

namespace UI.MainApp
{
    public class StoryEditorScreen : MonoBehaviour
    {
        [SerializeField] TabController tabs;
        [SerializeField] GameObject timeline;
        [SerializeField] GameObject itemList;
        [SerializeField] Transform itemListContainer;

        private void Start() {
            tabs.Init(OnTabClicked);
        }

        void OnTabClicked(int id) {

            switch (id) {
                case 0:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    break;
                case 1:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    break;
                case 2:
                    timeline.SetActive(false);
                    itemList.SetActive(true);
                    break;
                case 3:
                    timeline.SetActive(true);
                    itemList.SetActive(false);
                    break;
            }
        }
    }
}