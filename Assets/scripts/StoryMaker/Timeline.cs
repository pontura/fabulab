using System.Collections;
using System.Collections.Generic;
using UI.MainApp;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    // View of the keyframes. The current keyframe is owned by FilmMakerManager: this class only reads it
    // and asks FilmMakerManager to navigate.
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
        public float offset = 10;
        public float timer;
        public float keyframe_duration = 2;

        // True once the keyframes of the current story are built (and their thumbnails, when editing)
        public bool IsLoaded { get; private set; }

        int playingGameFrames;
        Coroutine thumbsRoutine;
        RectTransform canvasRect;

        ScenesManager scenesManager => ScenesManager.Instance;
        int activeKeyframe => filmMakerUI.CurrentKeyframe;

        // Width in pixels of the whole timeline
        public float total_x_marker
        {
            get
            {
                if (canvasRect == null)
                    canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
                return canvasRect.rect.width - offset;
            }
        }

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
        }

        void Start() {
            StoryMakerEvents.ChangeSpeed(scenesManager.currentFilmData.speed);
        }

        void OnDestroy()
        {
            StoryMakerEvents.OnLoadFilm -= Reset;
            StoryMakerEvents.OnStartNewStory -= Reset;
            StoryMakerEvents.ChangeSpeed -= ChangeSpeed;
        }

        void OnEnable() {
            StoryMakerEvents.OnStopDraw += OnStopDraw;
            StoryMakerEvents.UpdateDraw += UpdateDraw;
            if(all.Count>1)
                ghostImage.Show(true);

            shotButtons.Show(filmMakerUI.isEditing);

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
            StopAllCoroutines();
            ghostImage.Show(false);
        }

        // Keeps the keyframes and the marker fitted to the screen when it is resized
        void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled || all.Count == 0 || scenesManager == null)
                return;
            UpdateKeyframes();
            PlaceMarker();
        }

        void UpdateDraw()
        {
            if(!filmMakerUI.isEditing) return;
            if(!gameObject.activeSelf) return;

            if(activeKeyframe <= all.Count)
                all[activeKeyframe - 1].UpdateScreenshot();
        }

        // Snaps the dropped object to its position in the previous keyframe when it was released close to it
        void OnStopDraw(SceneObject so)
        {
            SOData soData = so.GetData();
            SceneElement sceneElement = scenesManager.GetSOInScene(activeKeyframe-2, soData.id);
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
            IsLoaded = false;
            if (thumbsRoutine != null)
                Scenario.Instance.StopCoroutine(thumbsRoutine);
            thumbsRoutine = null;

            ghostImage.Reset();
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
                thumbsRoutine = Scenario.Instance.StartCoroutine(CreateAllInitialThumbs());
            else
                OnLoadDone();
        }

        void OnLoadDone()
        {
            thumbsRoutine = null;
            filmMakerUI.ShowKeyframe(1);
            UpdateKeyframes();
            if(Data.Instance.gamesManager.playing)
            {
                Data.Instance.gamesManager.CheckIfNewGame();
            }
            Events.OnLoading(false);
            shotButtons.Show(false);
            OnStop();
            IsLoaded = true;
        }

        IEnumerator CreateAllInitialThumbs()
        {
            int id = 1;
            foreach (KeyFrameUI kf in all)
            {
                filmMakerUI.ShowKeyframe(id);
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
                if(id+1 == activeKeyframe)
                    kf.SetSelected(true);
                else if(kf.selected)
                    kf.SetSelected(false);
                id++;
            }
        }

        public void RemoveKeyframe(int keyframeId)
        {
            KeyFrameUI kf = all[keyframeId - 1];
            Destroy(kf.gameObject);
            all.Remove(kf);
            UpdateKeyframes();
        }

        public void OnKeyframeClicked(int keyframeId)
        {
            filmMakerUI.GoTo(keyframeId);
        }

        public void OnPlay() {
            shotButtons.Show(false);
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
                filmMakerUI.ShowKeyframe(activeKeyframe);
            }
            else
                CheckToNextKeyframe();
            keyFrameMarker.transform.localPosition = pos;
        }

        // While playing, shows the next keyframe when the timer reaches it
        void CheckToNextKeyframe()
        {
            float nextTimer = 0;
            for (int a = 0; a < activeKeyframe; a++)
            {
                if (a < all.Count)
                    nextTimer += all[a].duration;
            }
            if (timer >= nextTimer)
                filmMakerUI.ShowKeyframe(activeKeyframe + 1);
        }

        // Updates the marker, the selection and the keyframe panels to the current keyframe
        public void Refresh()
        {
            timer = CalculateTimer();
            SetPosition();
            UpdateKeyframes();
            ForceMarkerPosition();

            float activeDuration = all[activeKeyframe - 1].duration;
            float sliderValue = (activeDuration - Min_speed) / (Max_speed - Min_speed);

            durationBtn?.SetDuration(activeDuration);
            durationBtn?.SetValue(sliderValue);

            SetCams();
            UpdateGhostImage();
            CheckForKeyframeInteraction();
        }

        void UpdateGhostImage()
        {
            if(activeKeyframe<2) { ghostImage.Show(false); return; }
            int prevSceneID = activeKeyframe-2;
            if(scenesManager.Scenes[activeKeyframe-1].bgID == scenesManager.Scenes[prevSceneID].bgID)
            {
                ghostImage.Show(true);
                ghostImage.Init(all[prevSceneID].sprite);
            } else
            {
                ghostImage.Show(false);
            }
        }

        void SetCams()
        {
            CamData cd = scenesManager.Scenes[activeKeyframe-1].camData;
            StoryMakerEvents.SetCamDataEdition(cd.pos,cd.zoom);
        }

        void ForceMarkerPosition()
        {
            float pos_x = PlaceMarker();
            if(filmMakerUI.isEditing)
                shotButtons.OnMarkerUpdated(pos_x + all[activeKeyframe - 1].mid);
        }

        // Moves the marker to the start of the current keyframe and returns its x position
        float PlaceMarker()
        {
            Vector2 pos = keyFrameMarker.transform.localPosition;
            pos.x = CalculateTimer() * (total_x_marker / totalTimer);
            keyFrameMarker.transform.localPosition = pos;
            return pos.x;
        }

        float CalculateTimer()
        {
            float timer_pos = 0;
            for (int a = 0; a < activeKeyframe - 1; a++)
            {
                if (a < all.Count)
                    timer_pos += all[a].duration;
            }
            return timer_pos;
        }

        public float OnChangeDuration(float value) {
            float duration = Mathf.Lerp(Min_speed, Max_speed, value);
            all[activeKeyframe - 1].SetDuration(duration);
            SceneData activeScene = scenesManager.GetActiveScene();
            if (activeScene != null && activeScene.duration != duration) {
                activeScene.duration = duration;
                scenesManager.MarkChanged();
            }
            UpdateKeyframes();
            ForceMarkerPosition();
            return duration;
        }

        void CheckForKeyframeInteraction()
        {
            bool avoidInteraction = filmMakerUI.isPlayingGame && activeKeyframe <= playingGameFrames;
            gamesLockScreen.SetActive(avoidInteraction);
        }

        public void EnableStoryEdition(bool isEdition)
        {
            shotButtons.Show(isEdition);
            gamesLockScreen.SetActive(filmMakerUI.isPlayingGame);
            if(filmMakerUI.isPlayingGame)
                StartCoroutine(InitGame());
        }

        IEnumerator InitGame()
        {
            playingGameFrames = Data.Instance.gamesManager.totalLockedGameFrames;
            yield return new WaitUntil(() => Data.Instance.scenesData.loadedDone && IsLoaded);
            if(playingGameFrames > 0)
            {
                filmMakerUI.GoTo(playingGameFrames);
                yield return null;
                filmMakerUI.New();
            }
            else
            {
                // nada bloqueado: el primer frame de la plantilla queda editable, sin crear un frame nuevo automaticamente
                filmMakerUI.GoTo(1);
            }
        }
    }
}
