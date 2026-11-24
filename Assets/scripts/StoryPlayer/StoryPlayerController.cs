using BoardItems.BoardData;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace StoryPlayer
{
    // Standalone story player: receives the id of a story, loads it and plays it, without the app UI and
    // without login. Meant to be embedded in an HTML page (Unity WebGL).
    //
    // Input:
    //  - the "id" parameter of the page URL (?id=-Nxyz...), read on start
    //  - SendMessage from JavaScript to the GameObject "StoryPlayer":
    //      unityInstance.SendMessage('StoryPlayer', 'Load', '<storyId>');
    //      unityInstance.SendMessage('StoryPlayer', 'Play');   // also 'Pause', 'Restart'
    // Output (StoryPlayerWebBridge): loading, loaded, playing, paused, ended, error
    [RequireComponent(typeof(StoryPlayback))]
    public class StoryPlayerController : MonoBehaviour
    {
        [SerializeField] string databaseUrl = "https://fabu-lab-default-rtdb.firebaseio.com";
        [Tooltip("Story loaded when the URL has no id (to test in the editor)")]
        [SerializeField] string defaultStoryId;
        [SerializeField] bool autoPlay = true;
        [Tooltip("Seconds without the story loaded before telling the page it failed")]
        [SerializeField] float loadTimeout = 40;

        [Header("Playback (copied from the story editor by the scene builder)")]
        [SerializeField] ScenarioCameraManager scenarioCameraManager;
        [SerializeField] float delayFactor = 0.2f;
        [SerializeField] Vector2 limitsCamZoom1 = new Vector2(15, 20);
        [SerializeField] float minKeyframeDuration = 0.05f;

        StoryPlayback playback;
        StoryRestLoader loader;
        Coroutine loading;
        Coroutine watchdog;
        string loadedStoryId;
        // Second of the story shown while it is not playing (the playback clock only runs while playing)
        float stoppedTime;
        // A scrub started while the story was playing: it plays again when the scrub ends
        bool resumeAfterScrub;

        public bool IsLoaded => loadedStoryId != null;
        public bool IsPlaying => playback.IsPlaying;
        public bool IsScrubbing { get; private set; }

        // Second of the story being shown
        public float CurrentTime => playback.IsPlaying ? playback.Timer : stoppedTime;
        public float TotalDuration => IsLoaded ? playback.TotalDuration : 0;
        // 0..1
        public float Progress {
            get {
                float total = TotalDuration;
                return total > 0 ? Mathf.Clamp01(CurrentTime / total) : 0;
            }
        }

        void Awake() {
            playback = GetComponent<StoryPlayback>();
            playback.Finished += OnFinished;
            loader = new StoryRestLoader(this, databaseUrl);
        }

        void OnDestroy() {
            playback.Finished -= OnFinished;
        }

        IEnumerator Start() {
            StoryMakerEvents.SetEditing(false);

            Scenario scenario = Scenario.Instance != null ? Scenario.Instance : FindFirstObjectByType<Scenario>(FindObjectsInactive.Include);
            if (scenario == null) {
                Fail("The scene has no Scenario");
                yield break;
            }
            scenario.gameObject.SetActive(true);
            if (scenarioCameraManager == null)
                scenarioCameraManager = scenario.GetComponentInChildren<ScenarioCameraManager>(true);
            scenarioCameraManager.Init(false);
            playback.Configure(scenarioCameraManager, delayFactor, limitsCamZoom1, minKeyframeDuration);

            // Lets the data managers (DATA prefab) and the scenario run their Start
            yield return null;

            EnterStoryMode();

            string id = GetUrlParameter("id");
            if (string.IsNullOrEmpty(id))
                id = defaultStoryId;
            if (!string.IsNullOrEmpty(id))
                Load(id);
            else
                StoryPlayerWebBridge.Notify("waiting");
        }

        // Same setup the app does when it opens the story maker (StoryEditorPanel). The camera animator
        // has a state per zoom of the character editor: out of STORY it keeps moving the camera every frame
        void EnterStoryMode() {
            Events.Zoom(UI.ZoomStates.STORY, false);
            StoryMakerEvents.Restart();
            Events.ColorizeBG(PalettesManager.colorNames.BG_CELESTE);
        }

        void Update() {
            playback.Tick(Time.deltaTime);
        }

        // --- Called from JavaScript (SendMessage) ---

        public void Load(string storyId) {
            if (string.IsNullOrEmpty(storyId))
                return;
            if (loading != null)
                StopCoroutine(loading);
            playback.Stop();
            loadedStoryId = null;
            stoppedTime = 0;
            IsScrubbing = false;
            StoryPlayerWebBridge.Notify("loading", Payload(new { id = storyId }));
            loading = StartCoroutine(loader.LoadStory(storyId, OnStoryLoaded, Fail));
            if (watchdog != null)
                StopCoroutine(watchdog);
            watchdog = StartCoroutine(FailIfStillLoading(storyId));
        }

        // An exception inside the loading coroutine kills it without calling back: the page would wait forever
        IEnumerator FailIfStillLoading(string storyId) {
            yield return new WaitForSecondsRealtime(loadTimeout);
            watchdog = null;
            if (loading != null && loadedStoryId == null) {
                StopCoroutine(loading);
                Fail("Timed out loading story " + storyId + " (see the errors above in the console)");
            }
        }

        public void Play() {
            if (loadedStoryId == null)
                return;
            if (playback.CurrentKeyframe >= playback.KeyframesCount && playback.Timer >= playback.TotalDuration)
                ScenesManager.Instance.currentSceneId = 1;
            playback.Play();
            StoryPlayerWebBridge.Notify("playing", Payload(new { id = loadedStoryId }));
        }

        public void Pause() {
            if (!playback.IsPlaying)
                return;
            stoppedTime = playback.Timer;
            playback.Stop();
            playback.PauseNextFrame();
            StoryPlayerWebBridge.Notify("paused", Payload(new { id = loadedStoryId, keyframe = playback.CurrentKeyframe }));
        }

        public void Restart() {
            if (loadedStoryId == null)
                return;
            ScenesManager.Instance.currentSceneId = 1;
            Play();
        }

        // Pauses a playing story, plays a paused one. A paused story continues from the start of its keyframe
        public void TogglePause() {
            if (!IsLoaded)
                return;
            if (playback.IsPlaying)
                Pause();
            else
                Play();
        }

        // --- Scrub: moves keyframe by keyframe with the story paused ---

        public void BeginScrub() {
            if (!IsLoaded || IsScrubbing)
                return;
            IsScrubbing = true;
            resumeAfterScrub = playback.IsPlaying;
            Pause();
        }

        // Shows the next (direction > 0) or the previous keyframe. False when there is none
        public bool StepKeyframe(int direction) {
            if (!IsLoaded)
                return false;
            int keyframe = playback.CurrentKeyframe + (direction > 0 ? 1 : -1);
            if (keyframe < 1 || keyframe > playback.KeyframesCount)
                return false;
            if (playback.IsPlaying)
                Pause();
            playback.ShowKeyframe(keyframe);
            playback.ApplyKeyframeCamera(keyframe);
            stoppedTime = playback.GetStartTime(keyframe);
            StoryPlayerWebBridge.Notify("keyframe", Payload(new { id = loadedStoryId, keyframe }));
            return true;
        }

        public void EndScrub() {
            if (!IsScrubbing)
                return;
            IsScrubbing = false;
            if (resumeAfterScrub)
                Play();
        }

        // --- Loading ---

        // An exception here would leave the page waiting forever (it only ends with loaded or error)
        void OnStoryLoaded(LoadedStory story) {
            loading = null;
            try {
                ShowLoadedStory(story);
            } catch (Exception e) {
                Debug.LogException(e);
                Fail("Could not show story " + story.film?.id + ": " + e.Message);
            }
        }

        void ShowLoadedStory(LoadedStory story) {
            // The managers find these first, so they never go to the server for them (Items.LoadBoardItemForStory)
            CacheOthers(Data.Instance.charactersData.othersCharacters, story.characters, list => Data.Instance.charactersData.othersCharacters = list);
            CacheOthers(Data.Instance.sObjectsData.othersData, story.objects, list => Data.Instance.sObjectsData.othersData = list);

            playback.Stop();
            Scenario.Instance.sceneObjectsManager.ClearScene();

            ScenesManager scenesManager = ScenesManager.Instance;
            scenesManager.currentFilmData = story.film;
            scenesManager.currentFDataID = story.film.id;
            scenesManager.Scenes = story.scenes;
            scenesManager.currentSceneId = 1;
            Data.Instance.scenesData.currentFilmData = story.film;

            scenesManager.AddAllObjectsToScene();
            playback.ShowKeyframe(1, refreshAllObjects: true);
            playback.ApplyKeyframeCamera(1);

            loadedStoryId = story.film.id;
            stoppedTime = 0;
            StoryPlayerWebBridge.Notify("loaded", Payload(new {
                id = story.film.id,
                name = story.film.name,
                userID = story.film.userID,
                keyframes = story.scenes.Count,
                duration = playback.TotalDuration
            }));

            if (autoPlay)
                Play();
        }

        static void CacheOthers<T>(List<T> cache, List<T> loaded, Action<List<T>> setCache) where T : SOPartData {
            if (cache == null) {
                cache = new List<T>();
                setCache(cache);
            }
            foreach (T item in loaded) {
                cache.RemoveAll(x => x.id == item.id);
                cache.Add(item);
            }
        }

        void OnFinished() {
            stoppedTime = playback.TotalDuration;
            StoryMakerEvents.OnMovieOver();
            playback.ShowKeyframe(playback.CurrentKeyframe);
            StoryPlayerWebBridge.Notify("ended", Payload(new { id = loadedStoryId }));
        }

        void Fail(string message) {
            loading = null;
            Debug.LogError("[StoryPlayer] " + message);
            StoryPlayerWebBridge.Notify("error", Payload(new { message }));
        }

        static string Payload(object data) {
            return JsonConvert.SerializeObject(data);
        }

        // Reads a parameter of the page URL (WebGL). Empty outside the browser
        static string GetUrlParameter(string name) {
            string url = Application.absoluteURL;
            if (string.IsNullOrEmpty(url))
                return null;
            int queryStart = url.IndexOf('?');
            if (queryStart < 0)
                return null;
            string query = url.Substring(queryStart + 1);
            int hashStart = query.IndexOf('#');
            if (hashStart >= 0)
                query = query.Substring(0, hashStart);
            foreach (string pair in query.Split('&')) {
                string[] parts = pair.Split(new[] { '=' }, 2);
                if (parts.Length == 2 && Uri.UnescapeDataString(parts[0]) == name)
                    return Uri.UnescapeDataString(parts[1].Replace('+', ' '));
            }
            return null;
        }
    }
}
