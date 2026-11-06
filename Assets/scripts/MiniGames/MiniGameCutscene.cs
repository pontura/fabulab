using System;
using System.Collections;
using BoardItems;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace MiniGames
{
    // Plays a story as a cutscene of a minigame. It uses the real story player (StoryMaker screen),
    // like the onboarding does, but the Scenario camera renders into a texture shown in the minigame view.
    // The minigame screen stays open on top while the StoryMaker screen is active behind it
    public class MiniGameCutscene : MonoBehaviour
    {
        [SerializeField] int textureSize = 1024;
        [Tooltip("Frames to wait after the story loads before playing it (the story player UI initializes in its first frames)")]
        [SerializeField] int framesBeforePlay = 6;

        RenderTexture renderTexture;
        Camera scenarioCam;
        Action onOver;
        Coroutine routine;
        bool isReady;
        bool playWhenReady;

        public bool IsActive { get; private set; }

        public static bool Exists(string storyId)
        {
            if (string.IsNullOrEmpty(storyId))
                return false;
            ScenesData scenesData = Data.Instance.scenesData;
            return scenesData.userFilmsData.Exists(x => x.id == storyId) || scenesData.filmsData.Exists(x => x.id == storyId);
        }

        // Loads and plays a story. onOver is called when the story ends by itself
        public void Play(string storyId, RawImage view, Action onOver)
        {
            Load(storyId, view);
            PlayLoaded(onOver);
        }

        // Starts the loaded story (or as soon as it is ready). onOver is called when it ends by itself
        public void PlayLoaded(Action onOver)
        {
            if (!IsActive)
                return;
            this.onOver = onOver;
            if (isReady)
                StartPlayback();
            else
                playWhenReady = true;
        }

        // Loads a story and shows its first frame, paused
        public void Load(string storyId, RawImage view)
        {
            if (IsActive)
                Release();

            // Active before showing the StoryMaker screen, so the minigame screen knows it must stay open
            IsActive = true;
            isReady = false;
            playWhenReady = false;
            onOver = null;

            if (renderTexture == null) {
                renderTexture = new RenderTexture(textureSize, textureSize, 24) { name = "MiniGameCutsceneRT" };
                renderTexture.Create();
            }
            view.texture = renderTexture;

            Events.OnLoading(true);
            StoryMakerEvents.OnLoadFilm += OnLoadFilm;
            StoryMakerEvents.OnMovieOver += OnMovieOver;

            UIManager.Instance.boardUI.SetEditingType(BoardUI.editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.StoryMaker);

            ScenesData scenesData = Data.Instance.scenesData;
            if (scenesData.userFilmsData.Exists(x => x.id == storyId))
                scenesData.LoadUserFilm(storyId);
            else
                scenesData.LoadFilm(storyId);
        }

        // Leaves the story player and goes back to the minigame screen
        public void Stop()
        {
            if (!IsActive)
                return;
            Release();
            Events.ShowScreen(UIManager.screenType.MiniGame);
        }

        // Stops the story without changing the screen (ex: the minigame screen was closed with the back button)
        public void Release()
        {
            if (!IsActive)
                return;
            IsActive = false;
            isReady = false;
            playWhenReady = false;
            onOver = null;
            StoryMakerEvents.OnLoadFilm -= OnLoadFilm;
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
            if (routine != null) {
                StopCoroutine(routine);
                routine = null;
            }
            StoryMakerEvents.OnTimelinePlay(false);
            StoryMakerEvents.SetEditing(false);
            if (scenarioCam != null) {
                scenarioCam.targetTexture = null;
                scenarioCam = null;
            }
            Events.OnLoading(false);
        }

        void OnLoadFilm()
        {
            StoryMakerEvents.OnLoadFilm -= OnLoadFilm;
            routine = StartCoroutine(ReadyRoutine());
        }

        // Leaves the story in watch mode, paused on its first frame
        IEnumerator ReadyRoutine()
        {
            StoryMakerEvents.EnableStoryEdition(false);
            StoryMakerEvents.EnableInputManager(false);
            for (int i = 0; i < framesBeforePlay; i++)
                yield return null;

            scenarioCam = Scenario.Instance.Cam;
            scenarioCam.targetTexture = renderTexture;
            StoryMakerEvents.RewindStory(); // shows the first keyframe, stopped
            Events.OnLoading(false);
            routine = null;

            isReady = true;
            if (playWhenReady)
                StartPlayback();
        }

        void StartPlayback()
        {
            playWhenReady = false;
            ScenesManager.Instance.currentSceneId = 1;
            StoryMakerEvents.OnTimelinePlay(true);
        }

        void OnMovieOver()
        {
            if (!IsActive || onOver == null)
                return;
            // Called from inside the timeline update: the callback runs a bit later,
            // so the player finishes its own work before leaving the StoryMaker screen
            routine = StartCoroutine(MovieOverRoutine(onOver));
        }

        IEnumerator MovieOverRoutine(Action callback)
        {
            yield return null;
            yield return null;
            routine = null;
            if (IsActive)
                callback();
        }

        void OnDestroy()
        {
            StoryMakerEvents.OnLoadFilm -= OnLoadFilm;
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
            if (renderTexture != null) {
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }
    }
}
