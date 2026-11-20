using System.Collections;
using UI.MainApp;
using UI.MainApp.Home.User;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    // Editor UI of a story: navigation buttons, timeline and edition tools. The playback itself
    // (clock, keyframes, camera and avatar movements) is done by StoryPlayback
    public class FilmMakerManager : MonoBehaviour
    {
        public StoryPlayback.states State => playback.State;

        [SerializeField] Button nextAllButton;
        [SerializeField] Button prevAllButton;
        [SerializeField] Button nextButton;
        [SerializeField] Button prevButton;
        [SerializeField] Button playButton;
        [SerializeField] Button deleteButton;
        [SerializeField] Button newButton;
        [SerializeField] float delayFactor;
        [SerializeField] Toggle toggleTransition;
        [SerializeField] GameObject durationBtn;
        [SerializeField] Timeline timeline;
        [SerializeField, FormerlySerializedAs("videoPlayerFabulab")] VideoPlayer videoPlayer;
        [SerializeField] GameSelector gameSelector;
        [SerializeField] CamerasEditorUI camerasEditorUI;
        [SerializeField] ScenarioCameraManager scenarioCameraManager;
        [SerializeField] Vector2 limitsCamZoom1;

        public bool isEditing;
        public bool isPlayingGame;

        StoryPlayback playbackComponent;
        // Created on first use: it can be needed before Awake, by an event that reaches this object
        // while it is still inactive
        StoryPlayback playback {
            get {
                if (playbackComponent == null) {
                    playbackComponent = gameObject.AddComponent<StoryPlayback>();
                    playbackComponent.Configure(scenarioCameraManager, delayFactor, limitsCamZoom1, timeline.Min_speed);
                    playbackComponent.KeyframeDurationProvider = timeline.GetKeyframeDuration;
                    playbackComponent.KeyframeShown += OnKeyframeShown;
                    playbackComponent.Finished += OnPlaybackFinished;
                }
                return playbackComponent;
            }
        }

        ScenesManager scenesManager => ScenesManager.Instance;

        // The keyframe being shown (1 based). Only this class and StoryPlayback change it.
        public int CurrentKeyframe => scenesManager.currentSceneId;

        void Awake() {
            _ = playback;
            StoryMakerEvents.OnTimelinePlay += OnTimelinePlay;
            StoryMakerEvents.EnableStoryEdition += EnableStoryEdition;
            StoryMakerEvents.RewindStory += Rewind;
            StoryMakerEvents.OnMovieOver += OnMovieOver;
        }

        void OnDestroy() {
            if (playbackComponent != null) {
                playbackComponent.KeyframeShown -= OnKeyframeShown;
                playbackComponent.Finished -= OnPlaybackFinished;
            }

            StoryMakerEvents.OnTimelinePlay -= OnTimelinePlay;
            StoryMakerEvents.EnableStoryEdition -= EnableStoryEdition;
            StoryMakerEvents.RewindStory -= Rewind;
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
        }

        void Update()
        {
            if (!playback.IsPlaying)
                return;

            playback.Tick(Time.deltaTime);
            if (playback.IsPlaying)
                timeline.OnUpdate(playback.Timer);
        }

        // The story reached its end while playing
        void OnPlaybackFinished() {
            StoryMakerEvents.OnTimelinePlay(false);
            StoryMakerEvents.OnMovieOver();
            ShowKeyframe(CurrentKeyframe);
            timeline.PlaceMarkerAtEnd();
        }

        void EnableStoryEdition(bool enable) {
            isEditing = enable;
            deleteButton.gameObject.SetActive(enable);
            newButton.gameObject.SetActive(enable);
            toggleTransition.gameObject.SetActive(enable);
            videoPlayer.Show(!enable);
            durationBtn.gameObject.SetActive(enable);

            scenarioCameraManager.Init(enable);
            camerasEditorUI.Init(enable);
            camerasEditorUI.Show(enable);

            isPlayingGame = Data.Instance.gamesManager.IsEditing();

            if(isPlayingGame)
                gameSelector.Show(false);
            else
                gameSelector.Show(Data.Instance.gamesManager.activaGameData != "");

            if(enable || isPlayingGame)
                GetComponent<StoryEditorScreen>().Init();

            timeline.EnableStoryEdition(enable);
        }

        public void OnTimelinePlay(bool isOn)
        {
            if (isOn)
            {
                Play();
            }
            else
            {
                Stop();
                playback.PauseNextFrame();
            }
        }

        void Play() {
            timeline.OnPlay();
            timeline.Refresh();
            playback.Play();
        }

        public void PlayFromStart()
        {
            scenesManager.currentSceneId = 1;
            StoryMakerEvents.OnTimelinePlay(true);
        }

        void Stop() {
            timeline.OnStop();
            playButton.GetComponent<PlayButton>().SetButtons(false);
            playback.Stop();
        }

        void SetButtons()
        {
            int total = scenesManager.Scenes.Count;
            bool isFirst = scenesManager.currentSceneId == 1;
            bool isLast = scenesManager.currentSceneId >= total;

            prevButton.interactable = !isFirst;
            prevAllButton.interactable = !isFirst;
            nextButton.interactable = !isLast;
            nextAllButton.interactable = !isLast;
            deleteButton.interactable = total > 1;

            if (total > 1 || !isEditing)
                playButton.interactable = true;
        }

        // Adds a new keyframe after the current one
        public void New()
        {
            if (playback.IsPlaying)
                return;

            if (timeline.all.Count >= scenesManager.MaxKeyframes)
                return;

            scenesManager.AddNewScene(CurrentKeyframe + 1);
            Scenario.Instance.sceneObjectsManager.TurnOfAllTexts();
            SetButtons();
            timeline.AddNewKeyframe();
            timeline.Refresh();
        }

        // Deletes the current keyframe
        public void Delete()
        {
            if (playback.IsPlaying)
                return;

            if (Scenario.Instance != null)
                Scenario.Instance.sceneObjectsManager.ResetScene();

            int deletedKeyframe = CurrentKeyframe;
            scenesManager.RemoveActiveScene();
            timeline.RemoveKeyframe(deletedKeyframe);

            ShowKeyframe(Mathf.Max(deletedKeyframe - 1, 1), refreshAllObjects: true);
            timeline.Refresh();
        }

        public void Next()
        {
            if (CurrentKeyframe < scenesManager.Scenes.Count)
                GoTo(CurrentKeyframe + 1);
        }

        public void Prev()
        {
            GoTo(Mathf.Max(CurrentKeyframe - 1, 1));
        }

        public void PrevAll()
        {
            GoTo(1);
        }

        public void NextAll()
        {
            GoTo(scenesManager.Scenes.Count);
        }

        void Rewind()
        {
            GoTo(1);
        }

        void OnMovieOver()
        {
            if (isEditing)
                GoTo(scenesManager.Scenes.Count);
        }

        // Navigates to a keyframe: stops the playback, shows the keyframe and updates the timeline
        public void GoTo(int keyframe)
        {
            if (playback.IsPlaying)
                StoryMakerEvents.OnTimelinePlay(false);
            else
                Stop();

            ShowKeyframe(keyframe);
            timeline.Refresh();
        }

        // Shows the scene of a keyframe without touching the timeline. While playing, it also starts
        // the transition to the next one
        public void ShowKeyframe(int keyframe, bool refreshAllObjects = false)
        {
            playback.ShowKeyframe(keyframe, refreshAllObjects);
        }

        // Updates the editor UI to the keyframe just shown by the playback
        void OnKeyframeShown(int keyframe)
        {
            SceneData activeScene = scenesManager.GetActiveScene();
            if (activeScene != null) {
                timeline.keyframe_duration = Mathf.Max(activeScene.duration, timeline.Min_speed);
                toggleTransition.isOn = activeScene.transition;
            }
            SetButtons();
        }

        public void OnTransitionChange() {
            SceneData activeScene = scenesManager != null ? scenesManager.GetActiveScene() : null;
            if (activeScene == null || activeScene.transition == toggleTransition.isOn)
                return;
            activeScene.transition = toggleTransition.isOn;
            scenesManager.MarkChanged();
        }
    }
}
