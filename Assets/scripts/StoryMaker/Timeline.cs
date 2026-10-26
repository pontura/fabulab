using System.Collections;
using System.Collections.Generic;
using UI.MainApp;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class Timeline : MonoBehaviour
    {
        [SerializeField] DurationBtn durationBtn;
        [field: SerializeField] public float Min_speed { get; private set; } // 0.05 min speed 3 frames aprox
        [field: SerializeField] public float Max_speed { get; private set; } // 4;
        [SerializeField] GhostImage ghostImage;
        [SerializeField] GameObject gamesLockScreen;

        public FilmMakerManager filmMakerUI;
        public ShotButtons shotButtons;
        public KeyFrameUI keyframe;
        public Transform container;
        public List<KeyFrameUI> all;
        public KeyframeMarker keyFrameMarker;
        public float total_x_marker = 282;
        public float offset = 10;
        public float timer;
        public float keyframe_duration = 2;

        int activeAnimatedKeyframeID = 1;
        int playingGameFrames;

        ScenesManager scenesManager => ScenesManager.Instance;

        public float totalTimer
        {
            get
            {
                float t = 0;
                foreach(KeyFrameUI k in all)
                    t+= k.duration;
                return t;
            }
        }

        void Awake()
        {
            StoryMakerEvents.OnLoadFilm += Reset;
            StoryMakerEvents.OnStartNewStory += Reset;
            StoryMakerEvents.ChangeSpeed += ChangeSpeed;
            StoryMakerEvents.RewindStory += Rewind;
        }

        void Start() {
            StoryMakerEvents.ChangeSpeed(scenesManager.currentFilmData.speed);
            Invoke(nameof(SetTotalMarkers), Time.deltaTime * 3);
        }

        void OnDestroy()
        {
            StoryMakerEvents.OnLoadFilm -= Reset;
            StoryMakerEvents.OnStartNewStory -= Reset;
            StoryMakerEvents.ChangeSpeed -= ChangeSpeed;
            StoryMakerEvents.RewindStory -= Rewind;
        }

        void OnEnable() {
            StoryMakerEvents.OnStopDraw += OnStopDraw;
            StoryMakerEvents.UpdateDraw += UpdateDraw;
            StoryMakerEvents.OnMovieOver += OnMovieOver;
            if(all.Count>1)
                ghostImage.Show(true);

            Invoke(nameof(OnEnabledDelayed), 0.1f);

            if (all.Count == 0)
            {
                Reset();
            } else
            {
                UpdateKeyframes();
                StoryMakerEvents.UpdateDraw();
            }

            float duration = (keyframe_duration - Min_speed) / (Max_speed - Min_speed);
            durationBtn.Init(this, duration);
        }

        void OnDisable()
        {
            StoryMakerEvents.OnStopDraw -= OnStopDraw;
            StoryMakerEvents.UpdateDraw -= UpdateDraw;
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
            StopAllCoroutines();
            ghostImage.Show(false);
        }

        void OnEnabledDelayed()
        {
            shotButtons.Show(filmMakerUI.isEditing);
        }

        void SetTotalMarkers() {
            RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            total_x_marker = canvasRect.rect.width - offset;
        }

        void UpdateDraw()
        {
            if(!filmMakerUI.isEditing) return;
            if(!gameObject.activeSelf) return;

            if(activeAnimatedKeyframeID <= all.Count)
                all[activeAnimatedKeyframeID - 1].UpdateScreenshot();
        }

        // Snaps the dropped object to its position in the previous keyframe when it was released close to it
        void OnStopDraw(SceneObject so)
        {
            SOData soData = so.GetData();
            SceneElement sceneElement = scenesManager.GetSOInScene(scenesManager.currentSceneId-2, soData.id);
            if(sceneElement == null)
                return;
            V3 v3 = sceneElement.data.pos;
            Vector2 pos = new Vector2(v3.x, v3.y);
            float diff = Vector2.Distance(pos, so.gameObject.transform.localPosition);
            if(diff<3)
            {
                soData.pos = v3;
                so.gameObject.transform.localPosition = v3.ToVector3();
            }
        }

        void Rewind() {
            SetJumpWithoutSaving(1);
        }

        void OnMovieOver()
        {
            if(filmMakerUI.isEditing)
                SetJumpWithoutSaving(scenesManager.Scenes.Count);
        }

        void ChangeSpeed(int speed)
        {
            filmMakerUI.OnTimelinePlay(false);
            if(scenesManager.GetActiveScene()!=null)
                keyframe_duration = Mathf.Max(scenesManager.GetActiveScene().duration,Min_speed) - (speed);
            else
                keyframe_duration = scenesManager.Keyframe_default_duration - (speed);
        }

        // Rebuilds the keyframes from the scenes of the loaded story
        public void Reset()
        {
            ghostImage.Reset();
            activeAnimatedKeyframeID = 1;
            if (scenesManager.currentFilmData != null)
                StoryMakerEvents.ChangeSpeed(scenesManager.currentFilmData.speed);

            keyFrameMarker.transform.localPosition = Vector3.zero;
            timer = 0;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
            all.Clear();

            StopAllCoroutines();
            foreach (SceneData s in scenesManager.Scenes)
                AddNewKeyframe(s.duration);

            if(filmMakerUI.isEditing)
                Scenario.Instance.StartCoroutine(CreateAllInitialThumbs());
            else
                OnLoadDone();
        }

        void OnLoadDone()
        {
            filmMakerUI.JumpTo(1);
            UpdateKeyframes();
            if(Data.Instance.gamesManager.playing)
            {
                Data.Instance.gamesManager.CheckIfNewGame();
            }
            Events.OnLoading(false);
            shotButtons.Show(false);
            OnStop();
        }

        IEnumerator CreateAllInitialThumbs()
        {
            int id = 1;
            foreach (KeyFrameUI kf in all)
            {
                filmMakerUI.JumpTo(id);
                kf.UpdateScreenshot();
                id++;
                yield return new WaitForSeconds(0.1f);
            }
            OnLoadDone();
        }

        public void AddNewKeyframe(float duration = 0)
        {
            if (duration == 0) duration = keyframe_duration;
            KeyFrameUI kf = Instantiate(keyframe);
            kf.SetDuration(duration);
            kf.Init(this);
            all.Add(kf);
            kf.transform.SetParent(container);
            kf.transform.localScale = Vector2.one;
            kf.transform.localPosition = Vector2.zero;
        }

        void UpdateKeyframes()
        {
            int id = 0;
            float totalDuration = totalTimer;

            foreach (KeyFrameUI kf in all)
            {
                kf.SetID(id+1);
                kf.SetSize(totalDuration);
                kf.SetColor(id %2 == 0);
                if(id+1 == activeAnimatedKeyframeID)
                    kf.SetSelected(true);
                else if(kf.selected)
                    kf.SetSelected(false);
                id++;
            }
        }

        public void RemoveKeyframe()
        {
            KeyFrameUI kf = all[activeAnimatedKeyframeID - 1];
            Destroy(kf.gameObject);
            all.Remove(kf);
            UpdateKeyframes();
        }

        public void InitPlaying()
        {
            shotButtons.Show(false);
            Scenario.Instance.ResetScenario();
            JumpTo(scenesManager.currentSceneId);
            filmMakerUI.JumpTo(scenesManager.currentSceneId);
        }

        public void OnStop() {
            if(filmMakerUI.isEditing)
                shotButtons.Show(true);
        }

        public void OnUpdate()
        {
            timer += Time.deltaTime;
            SetPosition();
        }

        void SetPosition()
        {
            Vector2 pos = keyFrameMarker.transform.localPosition;
            pos.x = timer * total_x_marker / totalTimer;
            if (timer >= totalTimer)
            {
                StoryMakerEvents.OnTimelinePlay(false);
                pos.x = total_x_marker;
                StoryMakerEvents.OnMovieOver();
                filmMakerUI.JumpTo(activeAnimatedKeyframeID);
            }
            else
                CheckToNextAnimatedKeyframe();
            keyFrameMarker.transform.localPosition = pos;
        }

        // Saves the current keyframe and jumps to another one
        public void SetJump(int id) {
            StoryMakerEvents.OnSaveScene();
            SetJumpWithoutSaving(id);
        }

        void SetJumpWithoutSaving(int id) {
            scenesManager.currentSceneId = id;
            StoryMakerEvents.OnTimelineSetJump(activeAnimatedKeyframeID);
            JumpTo(id);
        }

        // Moves the marker and the selection to a keyframe, without changing the scene
        public void JumpTo(int keyframeId)
        {
            activeAnimatedKeyframeID = keyframeId;
            timer = CalculateTimer();
            SetPosition();
            UpdateKeyframes();
            ForceMarkerPosition();

            float activeDuration = all[activeAnimatedKeyframeID - 1].duration;
            float sliderValue = (activeDuration - Min_speed) / (Max_speed - Min_speed);

            durationBtn?.SetDuration(activeDuration);
            durationBtn?.SetValue(sliderValue);

            OnJumpDone();
        }

        void OnJumpDone()
        {
            SetCams();
            if(scenesManager.currentSceneId<2) { ghostImage.Show(false); return; }
            int prevSceneID = scenesManager.currentSceneId-2;
            if(scenesManager.Scenes[scenesManager.currentSceneId-1].bgID == scenesManager.Scenes[prevSceneID].bgID)
            {
                ghostImage.Show(true);
                ghostImage.Init(all[prevSceneID].sprite);
            } else
            {
                ghostImage.Show(false);
            }
            CheckForKeyframeInteraction();
        }

        void SetCams()
        {
            CamData cd = scenesManager.Scenes[scenesManager.currentSceneId-1].camData;
            StoryMakerEvents.SetCamDataEdition(cd.pos,cd.zoom);
        }

        void ForceMarkerPosition()
        {
            float timer_pos = CalculateTimer();
            Vector2 pos = keyFrameMarker.transform.localPosition;
            pos.x = timer_pos * (total_x_marker / totalTimer);
            keyFrameMarker.transform.localPosition = pos;
            if(filmMakerUI.isEditing)
                shotButtons.OnMarkerUpdated(pos.x + all[activeAnimatedKeyframeID - 1].mid);
        }

        float CalculateTimer()
        {
            float timer_pos = 0;
            for (int a = 0; a < activeAnimatedKeyframeID - 1; a++)
            {
                if (a < all.Count)
                    timer_pos += all[a].duration;
            }
            return timer_pos;
        }

        void CheckToNextAnimatedKeyframe()
        {
            float nextTimer = 0;
            for (int a = 0; a < activeAnimatedKeyframeID; a++)
            {
                if (a < all.Count)
                    nextTimer += all[a].duration;
            }
            if (timer >= nextTimer)
            {
                activeAnimatedKeyframeID++;
                filmMakerUI.JumpTo(activeAnimatedKeyframeID);
            }
        }

        public float OnChangeDuration(float value) {
            float duration = Mathf.Lerp(Min_speed, Max_speed, value);
            all[activeAnimatedKeyframeID - 1].SetDuration(duration);
            if (scenesManager.GetActiveScene() != null)
                scenesManager.GetActiveScene().duration = duration;
            UpdateKeyframes();
            ForceMarkerPosition();
            return duration;
        }

        void CheckForKeyframeInteraction()
        {
            bool avoidInteraction = filmMakerUI.isPlayingGame && scenesManager.currentSceneId <= playingGameFrames;
            gamesLockScreen.SetActive(avoidInteraction);
        }

        public void EnableStoryEdition(bool isEdition)
        {
            gamesLockScreen.SetActive(filmMakerUI.isPlayingGame);
            if(filmMakerUI.isPlayingGame)
                StartCoroutine(InitGame());
        }

        IEnumerator InitGame()
        {
            playingGameFrames = Data.Instance.gamesManager.totalLockedGameFrames;
            yield return new WaitForSeconds(0.5f);
            if(playingGameFrames > 0)
            {
                SetJump(playingGameFrames);
                yield return new WaitForSeconds(0.1f);
                filmMakerUI.New();
            }
            else
            {
                // nada bloqueado: el primer frame de la plantilla queda editable, sin crear un frame nuevo automaticamente
                SetJump(1);
            }
            yield return new WaitForSeconds(0.1f);
            CheckForKeyframeInteraction();
        }
    }
}
