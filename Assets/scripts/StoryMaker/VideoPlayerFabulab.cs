using BoardItems;
using UnityEngine;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class VideoPlayerFabulab : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_Text titleField;
        [SerializeField] TMPro.TMP_Text creatorField;
        [SerializeField] TMPro.TMP_Text dateField;
        [SerializeField] Image image;

        [SerializeField] FilmMakerManagerFabulab filmMakerManagerFabulab;
        [SerializeField] TimelineFabulab timeline;
        [SerializeField] GameObject playButton;

        private void Start()
        {
            StoryMakerEvents.OnMovieOver += OnMovieOver;
        }
        private void OnDestroy()
        {
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
        }
        void OnMovieOver()
        {
            playButton.SetActive(true);
        }
        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn)
            {
                playButton.SetActive(true);
                FilmDataFabulab f = Data.Instance.scenesData.currentFilmData;
                SetContent(f);
            }
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
        void SetContent(FilmDataFabulab content)
        {
            titleField.text = content.name;

            Data.Instance.cacheData.GetUser(content.userID, (userData) => {
                creatorField.text = userData.username;
                image.sprite = userData.thumb != null ? Sprite.Create(userData.thumb, new Rect(0, 0, userData.thumb.width, userData.thumb.height), new Vector2(0.5f, 0.5f)) : null;

            });

            dateField.text = content.GetTimestamp();

        }
    }
}
