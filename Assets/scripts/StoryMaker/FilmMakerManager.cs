using System.Collections;
using UI.MainApp;
using UI.MainApp.Home.User;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class FilmMakerManager : MonoBehaviour
    {
        public enum states
        {
            STOPPED,
            PLAYING
        }
        [field:SerializeField] public states State { private set; get;}

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

        float defaultZoom = 60;
        Coroutine pauseRoutine;

        ScenesManager scenesManager => ScenesManager.Instance;

        // The keyframe being shown (1 based). This class is the only one that changes it.
        public int CurrentKeyframe => scenesManager.currentSceneId;

        void Awake() {
            StoryMakerEvents.OnTimelinePlay += OnTimelinePlay;
            StoryMakerEvents.EnableStoryEdition += EnableStoryEdition;
            StoryMakerEvents.RewindStory += Rewind;
            StoryMakerEvents.OnMovieOver += OnMovieOver;
        }

        void OnDestroy() {
            StoryMakerEvents.OnTimelinePlay -= OnTimelinePlay;
            StoryMakerEvents.EnableStoryEdition -= EnableStoryEdition;
            StoryMakerEvents.RewindStory -= Rewind;
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
        }

        void Update()
        {
            if (State == states.STOPPED)
                return;

            timeline.OnUpdate();
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

            if(enable || Data.Instance.gamesManager.IsEditing())
            {
                GetComponent<StoryEditorScreen>().Init();
                timeline.GetComponent<Animator>().Play("edit");
            }
            else
                timeline.GetComponent<Animator>().Play("player");

            timeline.EnableStoryEdition(enable);
        }

        public void OnTimelinePlay(bool isOn)
        {
            StopAllCoroutines();
            pauseRoutine = null;
            if (isOn)
            {
                Play();
            }
            else
            {
                Stop();
                PauseNextFrame();
            }
        }

        void Play() {
            State = states.PLAYING;
            timeline.OnPlay();
            Scenario.Instance.ResetScenario();
            timeline.Refresh();
            ShowKeyframe(CurrentKeyframe);
            StoryMakerEvents.ReorderSceneObjectsInZ();
        }

        public void PlayFromStart()
        {
            scenesManager.currentSceneId = 1;
            StoryMakerEvents.OnTimelinePlay(true);
        }

        void Stop() {
            timeline.OnStop();
            playButton.GetComponent<PlayButton>().SetButtons(false);
            State = states.STOPPED;
            iTween.Stop();
        }

        // Freezes tweens and animations one frame later, once the shown keyframe has applied its poses.
        // Starting the playback cancels it (OnTimelinePlay stops all coroutines).
        void PauseNextFrame() {
            if (!isActiveAndEnabled) {
                SetPaused();
                return;
            }
            if (pauseRoutine != null)
                StopCoroutine(pauseRoutine);
            pauseRoutine = StartCoroutine(PauseRoutine());
        }

        IEnumerator PauseRoutine() {
            yield return null;
            pauseRoutine = null;
            SetPaused();
        }

        void SetPaused() {
            iTween.Stop();
            StoryMakerEvents.OnMoviePaused();
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
            if (State == states.PLAYING)
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
            if (State == states.PLAYING)
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
            if (State == states.PLAYING)
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
            int total = scenesManager.Scenes.Count;
            int lastSceneId = refreshAllObjects ? -1 : scenesManager.currentSceneId;
            scenesManager.currentSceneId = Mathf.Clamp(keyframe, 1, Mathf.Max(total, 1));

            int nextSceneid = scenesManager.currentSceneId + 1;
            SceneData activeScene = scenesManager.GetActiveScene();
            if (activeScene != null)
                timeline.keyframe_duration = Mathf.Max(activeScene.duration, timeline.Min_speed);

            if (State == states.PLAYING && nextSceneid <= total)
            {
                string backgroundID = scenesManager.GetBackground(scenesManager.currentSceneId);
                string nextBackgroundID = scenesManager.GetBackground(nextSceneid);
                string prevBackgroundID = "";

                if(scenesManager.currentSceneId>1)
                    prevBackgroundID = scenesManager.GetBackground(scenesManager.currentSceneId - 1);

                if (backgroundID == nextBackgroundID)
                {
                    float delay = timeline.keyframe_duration * delayFactor;
                    StartCoroutine(MoveAvatarsAfter(delay));
                    StartCoroutine(MoveCamera(timeline.keyframe_duration));
                } else if (backgroundID != prevBackgroundID)
                {
                    SetCameraNewScene();
                }
            } else if(State == states.STOPPED) {
                PauseNextFrame();
            } else
            {
                MoveCamSingleFrame();
            }

            SetButtons();

            if (scenesManager.GetActiveScene() != null) {
                toggleTransition.isOn = activeScene.transition;
                if (Scenario.Instance != null)
                    Scenario.Instance.sceneObjectsManager.SetBackgroundLights();
            }

            if (activeScene != null && Scenario.Instance != null) {
                SceneData previousScene = lastSceneId != scenesManager.currentSceneId ? scenesManager.GetScene(lastSceneId) : null;
                Scenario.Instance.sceneObjectsManager.ShowScene(activeScene, previousScene);
            }
            StoryMakerEvents.ReorderSceneObjectsInZ();
        }

        void MoveCamSingleFrame()
        {
            CamData currentScene = scenesManager.Scenes[scenesManager.currentSceneId-1].camData;
            Vector2 pos_from = GetPos(currentScene.pos, currentScene.zoom);
            scenarioCameraManager.OnUpdate(pos_from, currentScene.zoom);
        }

        void SetCameraNewScene()
        {
            CamData currentScene = scenesManager.Scenes[scenesManager.currentSceneId-1].camData;
            if(currentScene.zoom < 1) currentScene.zoom = defaultZoom;
            scenarioCameraManager.OnUpdate(currentScene.pos, currentScene.zoom);
        }

        IEnumerator MoveCamera(float duration)
        {
            if(scenesManager.currentSceneId>=scenesManager.Scenes.Count)
                yield break;

            CamData currentScene = scenesManager.Scenes[scenesManager.currentSceneId-1].camData;
            CamData nextScene = scenesManager.Scenes[scenesManager.currentSceneId].camData;

            if(currentScene.zoom < 1) currentScene.zoom = defaultZoom;
            if(nextScene.zoom < 1) nextScene.zoom = defaultZoom;

            Vector2 pos_from = GetPos(currentScene.pos, currentScene.zoom);
            if(!currentScene.tween)
            {
                scenarioCameraManager.OnUpdate(pos_from, currentScene.zoom);
                yield break;
            }

            Vector2 pos_to = GetPos(nextScene.pos, nextScene.zoom);
            float a = 0;
            while(a<duration)
            {
                float d = a/duration;
                Vector2 pos = Vector2.Lerp(pos_from, pos_to, d);
                float zoom = Mathf.Lerp(currentScene.zoom, nextScene.zoom, d);

                scenarioCameraManager.OnUpdate(pos, zoom);
                yield return new WaitForEndOfFrame();
                a += Time.deltaTime;
            }
        }

        Vector2 GetPos(Vector2 pos, float zoom)
        {
            if(zoom == defaultZoom || zoom == 0)
                return Data.Instance.settings.camDatas[0].pos;

            pos.x = limitsCamZoom1.x * pos.x;
            pos.y = (limitsCamZoom1.y * pos.y) + 10;
            return pos;
        }

        IEnumerator MoveAvatarsAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (State == states.PLAYING)
                Scenario.Instance.movementManager.MoveElements(scenesManager.GetActiveScene(), scenesManager.GetNextActiveScene(), timeline.keyframe_duration - delay);
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
