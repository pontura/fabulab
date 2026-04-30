using Google.MiniJSON;
using UnityEngine;
namespace Yaguar.StoryMaker.Editor
{
    public class VideoPlayerFabulab : MonoBehaviour
    {
        [SerializeField] FilmMakerManagerFabulab filmMakerManagerFabulab;
        [SerializeField] TimelineFabulab timeline;
        [SerializeField] GameObject playButton;

        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
        }
        public void PlayClicked()
        {
            playButton.SetActive(false);
            ScenesManager.Instance.currentSceneId = 1;
            timeline.JumpTo(ScenesManager.Instance.currentSceneId);
            StoryMakerEvents.OnTimelinePlay(true);
        }
        public void FastForward()
        {
            filmMakerManagerFabulab.Next();
            StoryMakerEvents.OnTimelinePlay(true);
        }
        public void Rewind()
        {
            filmMakerManagerFabulab.Prev();
            StoryMakerEvents.OnTimelinePlay(true);
        }
    }
}
