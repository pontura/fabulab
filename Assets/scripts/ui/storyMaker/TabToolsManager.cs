using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp
{
    public class TabToolsManager : MonoBehaviour
    {
        [SerializeField] GameObject backgrounds;
        [SerializeField] GameObject timeline;

        void Start()
        {
            backgrounds.SetActive(false);
        }
        public void SetOn(int tabID)
        {
            backgrounds.SetActive(false);
            timeline.SetActive(false);
            switch (tabID)
            {
                case 0:
                    backgrounds.SetActive(true);
                    break;
                case 4:
                    FilmMakerManager f = GetComponentInParent<FilmMakerManager>();
                    if(f != null && f.isEditing)
                        timeline.SetActive(true);
                    break;
            }
        }
    }
}
