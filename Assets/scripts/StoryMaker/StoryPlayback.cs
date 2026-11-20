using System;
using System.Collections;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    // Plays the story loaded in ScenesManager: shows its keyframes over time, moving the camera and the
    // avatars between them. It has no UI, so it is shared by the story editor (FilmMakerManager) and the
    // standalone story player. The keyframe being shown is ScenesManager.currentSceneId (1 based)
    public class StoryPlayback : MonoBehaviour
    {
        public enum states
        {
            STOPPED,
            PLAYING
        }

        [SerializeField] ScenarioCameraManager scenarioCameraManager;
        // Part of a keyframe that passes before its avatars start moving to the next one
        [SerializeField] float delayFactor;
        [SerializeField] Vector2 limitsCamZoom1;
        [SerializeField] float minKeyframeDuration = 0.05f;

        public states State { get; private set; }
        public bool IsPlaying => State == states.PLAYING;

        // Seconds since the start of the story
        public float Timer { get; private set; }

        // Duration of a keyframe (1 based). By default it is the one saved in the scene; the editor
        // replaces it with the durations of its timeline
        public Func<int, float> KeyframeDurationProvider { get; set; }

        // A keyframe was shown (also when stopped)
        public event Action<int> KeyframeShown;
        // The playback reached the end of the story. It is already stopped
        public event Action Finished;

        const float defaultZoom = 60;
        Coroutine pauseRoutine;

        ScenesManager scenesManager => ScenesManager.Instance;
        public int CurrentKeyframe => scenesManager.currentSceneId;
        public int KeyframesCount => scenesManager.Scenes.Count;

        public void Configure(ScenarioCameraManager scenarioCameraManager, float delayFactor, Vector2 limitsCamZoom1, float minKeyframeDuration) {
            this.scenarioCameraManager = scenarioCameraManager;
            this.delayFactor = delayFactor;
            this.limitsCamZoom1 = limitsCamZoom1;
            this.minKeyframeDuration = minKeyframeDuration;
        }

        public float GetKeyframeDuration(int keyframe) {
            if (KeyframeDurationProvider != null)
                return KeyframeDurationProvider(keyframe);
            SceneData scene = scenesManager.GetScene(keyframe);
            if (scene == null)
                return 0;
            return scene.duration > 0 ? scene.duration : scenesManager.Keyframe_default_duration;
        }

        // Second of the story where a keyframe starts
        public float GetStartTime(int keyframe) {
            float time = 0;
            for (int k = 1; k < keyframe; k++)
                time += GetKeyframeDuration(k);
            return time;
        }

        public float TotalDuration => GetStartTime(KeyframesCount + 1);

        // Plays from the current keyframe
        public void Play() {
            StopRoutines();
            State = states.PLAYING;
            Timer = GetStartTime(CurrentKeyframe);
            if (Scenario.Instance != null)
                Scenario.Instance.ResetScenario();
            ShowKeyframe(CurrentKeyframe);
        }

        public void Stop() {
            StopRoutines();
            State = states.STOPPED;
            iTween.Stop();
        }

        // Advances the clock while playing, showing the next keyframe when its time comes
        public void Tick(float deltaTime) {
            if (!IsPlaying)
                return;

            Timer += deltaTime;
            float total = TotalDuration;
            if (Timer >= total) {
                Timer = total;
                Stop();
                Finished?.Invoke();
                return;
            }
            if (CurrentKeyframe < KeyframesCount && Timer >= GetStartTime(CurrentKeyframe + 1))
                ShowKeyframe(CurrentKeyframe + 1);
        }

        // Freezes tweens and animations one frame later, once the shown keyframe has applied its poses.
        // Playing or stopping cancels it
        public void PauseNextFrame() {
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

        void StopRoutines() {
            StopAllCoroutines();
            pauseRoutine = null;
        }

        // Shows the scene of a keyframe. While playing, it also starts the transition to the next one.
        // refreshAllObjects shows every object of the keyframe, not only the ones that changed
        public void ShowKeyframe(int keyframe, bool refreshAllObjects = false) {
            int total = KeyframesCount;
            int lastSceneId = refreshAllObjects ? -1 : CurrentKeyframe;
            scenesManager.currentSceneId = Mathf.Clamp(keyframe, 1, Mathf.Max(total, 1));

            int nextSceneId = CurrentKeyframe + 1;
            SceneData activeScene = scenesManager.GetActiveScene();
            float duration = Mathf.Max(activeScene != null ? activeScene.duration : 0, minKeyframeDuration);

            if (IsPlaying && nextSceneId <= total) {
                string backgroundID = scenesManager.GetBackground(CurrentKeyframe);
                string nextBackgroundID = scenesManager.GetBackground(nextSceneId);
                string prevBackgroundID = CurrentKeyframe > 1 ? scenesManager.GetBackground(CurrentKeyframe - 1) : "";

                if (backgroundID == nextBackgroundID) {
                    float delay = duration * delayFactor;
                    StartCoroutine(MoveAvatarsAfter(delay, duration));
                    StartCoroutine(MoveCamera(duration));
                } else if (backgroundID != prevBackgroundID) {
                    SetCameraNewScene();
                }
            } else if (!IsPlaying) {
                PauseNextFrame();
            } else {
                ApplyKeyframeCamera(CurrentKeyframe);
            }

            if (activeScene != null && Scenario.Instance != null) {
                Scenario.Instance.sceneObjectsManager.SetBackgroundLights();
                SceneData previousScene = lastSceneId != CurrentKeyframe ? scenesManager.GetScene(lastSceneId) : null;
                Scenario.Instance.sceneObjectsManager.ShowScene(activeScene, previousScene);
            }

            KeyframeShown?.Invoke(CurrentKeyframe);
            StoryMakerEvents.ReorderSceneObjectsInZ();
        }

        // Puts the camera as it is framed in a keyframe
        public void ApplyKeyframeCamera(int keyframe) {
            SceneData scene = scenesManager.GetScene(keyframe);
            if (scene == null || scene.camData == null)
                return;
            CamData camData = scene.camData;
            scenarioCameraManager.OnUpdate(GetPos(camData.pos, camData.zoom), camData.zoom);
        }

        void SetCameraNewScene() {
            CamData camData = scenesManager.GetActiveScene().camData;
            if (camData.zoom < 1) camData.zoom = defaultZoom;
            scenarioCameraManager.OnUpdate(camData.pos, camData.zoom);
        }

        IEnumerator MoveCamera(float duration) {
            if (CurrentKeyframe >= KeyframesCount)
                yield break;

            CamData currentScene = scenesManager.Scenes[CurrentKeyframe - 1].camData;
            CamData nextScene = scenesManager.Scenes[CurrentKeyframe].camData;

            if (currentScene.zoom < 1) currentScene.zoom = defaultZoom;
            if (nextScene.zoom < 1) nextScene.zoom = defaultZoom;

            Vector2 pos_from = GetPos(currentScene.pos, currentScene.zoom);
            if (!currentScene.tween) {
                scenarioCameraManager.OnUpdate(pos_from, currentScene.zoom);
                yield break;
            }

            Vector2 pos_to = GetPos(nextScene.pos, nextScene.zoom);
            float a = 0;
            while (a < duration) {
                float d = a / duration;
                Vector2 pos = Vector2.Lerp(pos_from, pos_to, d);
                float zoom = Mathf.Lerp(currentScene.zoom, nextScene.zoom, d);

                scenarioCameraManager.OnUpdate(pos, zoom);
                yield return new WaitForEndOfFrame();
                a += Time.deltaTime;
            }
        }

        Vector2 GetPos(Vector2 pos, float zoom) {
            if (zoom == defaultZoom || zoom == 0)
                return Data.Instance.settings.camDatas[0].pos;

            pos.x = limitsCamZoom1.x * pos.x;
            pos.y = (limitsCamZoom1.y * pos.y) + 10;
            return pos;
        }

        IEnumerator MoveAvatarsAfter(float delay, float duration) {
            yield return new WaitForSeconds(delay);
            if (IsPlaying)
                Scenario.Instance.movementManager.MoveElements(scenesManager.GetActiveScene(), scenesManager.GetNextActiveScene(), duration - delay);
        }
    }
}
