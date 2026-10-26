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

        ScenesManager scenesManager => ScenesManager.Instance;

        void Awake() {
            StoryMakerEvents.OnTimelinePlay += OnTimelinePlay;
            StoryMakerEvents.EnableStoryEdition += EnableStoryEdition;
            StoryMakerEvents.OnTimelineSetJump += OnTimelineSetJump;
        }

        void OnDestroy() {
            StoryMakerEvents.OnTimelinePlay -= OnTimelinePlay;
            StoryMakerEvents.EnableStoryEdition -= EnableStoryEdition;
            StoryMakerEvents.OnTimelineSetJump -= OnTimelineSetJump;
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
            if (isOn)
            {
                Play();
            }
            else
            {
                Stop();
                Invoke(nameof(SetPaused), Time.deltaTime);
            }
        }

        void Play() {
            if (isEditing)
                StoryMakerEvents.OnSaveScene();
            State = states.PLAYING;
            timeline.InitPlaying();
            StoryMakerEvents.ReorderSceneObjectsInZ();
        }

        void Stop() {
            timeline.OnStop();
            playButton.GetComponent<PlayButton>().SetButtons(false);
            State = states.STOPPED;
            iTween.Stop();
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

            StoryMakerEvents.OnSaveScene();

            scenesManager.currentSceneId++;
            scenesManager.AddNewScene(scenesManager.currentSceneId);
            Scenario.Instance.sceneObjectsManager.TurnOfAllTexts();
            SetButtons();
            timeline.AddNewKeyframe();
            timeline.JumpTo(scenesManager.currentSceneId);
        }

        // Deletes the current keyframe
        public void Delete()
        {
            if (State == states.PLAYING)
                return;

            if (Scenario.Instance != null)
                Scenario.Instance.sceneObjectsManager.ResetScene();

            scenesManager.RemoveActiveScene();
            if (scenesManager.currentSceneId > 1)
                scenesManager.currentSceneId--;

            SetButtons();
            scenesManager.SetSceneObjectsIntoScenario();

            timeline.RemoveKeyframe();

            timeline.JumpTo(scenesManager.currentSceneId);
        }

        public void Next()
        {
            Stop();

            if(isEditing)
            {
                StoryMakerEvents.OnTimelinePlay(false);
                StoryMakerEvents.OnSaveScene();
            }

            if (scenesManager.currentSceneId < scenesManager.Scenes.Count)
                GoToScene(scenesManager.currentSceneId + 1);
        }

        public void Prev()
        {
            Stop();
            StoryMakerEvents.OnSaveScene();
            GoToScene(Mathf.Max(scenesManager.currentSceneId - 1, 1));
        }

        public void PrevAll()
        {
            Stop();
            StoryMakerEvents.OnSaveScene();
            GoToScene(1);
        }

        public void NextAll()
        {
            Stop();
            StoryMakerEvents.OnSaveScene();
            GoToScene(scenesManager.Scenes.Count);
        }

        void GoToScene(int sceneId)
        {
            JumpTo(sceneId);
            timeline.JumpTo(scenesManager.currentSceneId);
        }

        void OnTimelineSetJump(int lastSceneId) {
            Stop();
            SetButtons();

            if (scenesManager.GetActiveScene() != null) {
                toggleTransition.isOn = scenesManager.GetActiveScene().transition;
                StoryMakerEvents.SetBackgroundLights();
            }

            scenesManager.SetSceneObjectsIntoScenario(lastSceneId);
            StoryMakerEvents.ReorderSceneObjectsInZ();

            Invoke(nameof(SetPaused), Time.deltaTime);
        }

        // Shows the scene of a keyframe. While playing, it also starts the transition to the next one
        public void JumpTo(int keyframeID)
        {
            int lastSceneId = scenesManager.currentSceneId;
            scenesManager.currentSceneId = keyframeID;

            int total = scenesManager.Scenes.Count;
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
                Invoke(nameof(SetPaused), Time.deltaTime);
            } else
            {
                MoveCamSingleFrame();
            }

            if (scenesManager.currentSceneId > total)
                scenesManager.currentSceneId = total;
            else if (scenesManager.currentSceneId <= 1)
                scenesManager.currentSceneId = 1;

            SetButtons();

            if (scenesManager.GetActiveScene() != null) {
                toggleTransition.isOn = activeScene.transition;
                StoryMakerEvents.SetBackgroundLights();
            }

            scenesManager.SetSceneObjectsIntoScenario(lastSceneId);
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
                scenesManager.GetActiveScene().MoveElements(timeline.keyframe_duration - delay);
        }

        public void OnTransitionChange() {
            if(scenesManager!=null && scenesManager.GetActiveScene()!=null)
                scenesManager.GetActiveScene().transition = toggleTransition.isOn;
        }
    }
}
