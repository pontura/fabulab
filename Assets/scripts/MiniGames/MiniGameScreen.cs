using BoardItems.BoardData;
using System.Collections.Generic;
using UI;
using UI.MainApp;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames
{
    // Screen that hosts a minigame. Flow:
    // 1. Poster: first frame of the intro story (a story of the user), paused + PLAY
    // 2. Intro: that story plays + SKIP INTRO
    // 3. The game
    // 4. After losing: final cutscene (a story of the user) + GAME OVER, which goes back to the poster
    // The stories are shown in one RawImage (view) and the game in another one (gameview)
    public class MiniGameScreen : MainScreen
    {
        [SerializeField] MiniGameStage stage;
        [Tooltip("Shows the stories (poster, intro and final cutscene)")]
        [SerializeField] RawImage view;
        [Tooltip("Shows the game")]
        [SerializeField] RawImage gameview;
        [SerializeField] MiniGame miniGame;

        [Header("Generic action button")]
        [SerializeField] Button actionButton;
        [SerializeField] TMPro.TMP_Text actionLabel;
        [SerializeField] string playText = "PLAY";
        [SerializeField] string skipIntroText = "SKIP INTRO";
        [SerializeField] string gameOverText = "GAME OVER";

        [Tooltip("Optional")]
        [SerializeField] TMPro.TMP_Text scoreField;
        [Tooltip("Seconds after losing before the final cutscene starts")]
        [SerializeField] float gameOverDelay = 1.5f;

        // Used when the user has no characters (same template used to create a new character)
        [SerializeField] string defaultCharacterId = "-On3wQ6Vy9jnpMtTTgWb";

        enum Step { Closed, Poster, Intro, Playing, Outro }
        Step step = Step.Closed;

        MiniGameCutscene cutscene;
        bool initialized;

        void Init()
        {
            if (initialized) return;
            initialized = true;
            cutscene = GetComponent<MiniGameCutscene>();
            if (cutscene == null)
                cutscene = gameObject.AddComponent<MiniGameCutscene>();
            actionButton.onClick.AddListener(OnActionClicked);
        }

        protected override void ShowScreen(UIManager.screenType type)
        {
            if (type == UIManager.screenType.MiniGame) {
                if (step == Step.Closed)
                    Open();
                // else: coming back from a cutscene, the flow already handles it
            } else if (type == UIManager.screenType.StoryMaker && cutscene != null && cutscene.IsActive) {
                // A cutscene is playing in the story player, behind this screen
            } else if (step != Step.Closed) {
                Close();
            }
        }

        void Open()
        {
            Init();
            Show(true);
            stage.Open(gameview);
            ShowMovieView(false);
            miniGame.OnScoreChanged += OnScoreChanged;
            miniGame.OnFinished += OnFinished;
            // The poster changes to the StoryMaker screen: it waits until every listener got this screen change
            step = Step.Poster;
            actionButton.gameObject.SetActive(false);
            StartCoroutine(ShowPosterNextFrame());
        }

        System.Collections.IEnumerator ShowPosterNextFrame()
        {
            yield return null;
            if (step == Step.Poster)
                ShowPoster();
        }

        void Close()
        {
            step = Step.Closed;
            CancelInvoke();
            StopAllCoroutines();
            miniGame.OnScoreChanged -= OnScoreChanged;
            miniGame.OnFinished -= OnFinished;
            miniGame.Stop();
            cutscene.Release();
            stage.Close();
            Show(false);
        }

        void OnActionClicked()
        {
            switch (step) {
                case Step.Poster: StartIntro(); break;
                case Step.Intro: EndIntro(); break;
                case Step.Outro: EndOutro(); break;
            }
        }

        // 1. Poster: the first frame of the intro story, paused
        void ShowPoster()
        {
            SetStep(Step.Poster);
            CancelInvoke();
            miniGame.Stop();
            stage.Clear();
            stage.ResetBackgroundColor();

            string storyId = miniGame.IntroStoryId;
            if (MiniGameCutscene.Exists(storyId)) {
                stage.SetRendering(false);
                ShowMovieView(true);
                cutscene.Load(storyId, view);
            } else {
                // Without an intro story, PLAY is shown over the empty stage and starts the game
                if (!string.IsNullOrEmpty(storyId))
                    Debug.LogWarning("MiniGameScreen: intro story not found: " + storyId);
                stage.SetRendering(true);
                ShowMovieView(false);
            }
        }

        // 2. Intro: plays the story loaded in the poster
        void StartIntro()
        {
            if (!cutscene.IsActive) {
                StartGame();
                return;
            }
            SetStep(Step.Intro);
            cutscene.PlayLoaded(EndIntro);
        }

        void EndIntro()
        {
            if (step != Step.Intro) return;
            StopCutscene();
            StartGame();
        }

        // 3. Game
        void StartGame()
        {
            SetStep(Step.Playing);
            stage.Clear();
            stage.ResetBackgroundColor();
            stage.SetRendering(true);
            ShowMovieView(false);
            miniGame.Init(stage, GetCharacterId());
            miniGame.Play();
        }

        void OnFinished(int score)
        {
            Invoke(nameof(StartOutro), gameOverDelay);
        }

        // 4. Final cutscene
        void StartOutro()
        {
            if (step != Step.Playing) return;
            SetStep(Step.Outro);

            string storyId = miniGame.OutroStoryId;
            if (!MiniGameCutscene.Exists(storyId)) {
                // Without a final story, GAME OVER is shown over the game
                if (!string.IsNullOrEmpty(storyId))
                    Debug.LogWarning("MiniGameScreen: final story not found: " + storyId);
                return;
            }
            stage.SetRendering(false);
            ShowMovieView(true);
            cutscene.Play(storyId, view, null); // stays on its last frame until GAME OVER is clicked
        }

        void EndOutro()
        {
            StopCutscene();
            ShowPoster();
        }

        void StopCutscene()
        {
            if (!cutscene.IsActive) return;
            cutscene.Stop();
            // The cutscenes add the StoryMaker screen to the back history: back from here goes home
            UIManager.Instance.AddBackTo(UIManager.screenType.Home, true);
            UIManager.Instance.AddBackTo(UIManager.screenType.MiniGame);
        }

        // Shows the stories RawImage or the game RawImage
        void ShowMovieView(bool isOn)
        {
            view.gameObject.SetActive(isOn);
            gameview.gameObject.SetActive(!isOn);
        }

        void SetStep(Step newStep)
        {
            step = newStep;
            string text = null;
            switch (step) {
                case Step.Poster: text = playText; break;
                case Step.Intro: text = skipIntroText; break;
                case Step.Outro: text = gameOverText; break;
            }
            actionButton.gameObject.SetActive(text != null);
            if (actionLabel != null && text != null)
                actionLabel.text = text;
            if (scoreField != null)
                scoreField.gameObject.SetActive(step == Step.Playing || step == Step.Outro);
        }

        void OnScoreChanged(int score)
        {
            if (scoreField != null)
                scoreField.text = score.ToString();
        }

        // The character set in the minigame. Otherwise, the first character of the logged user's list,
        // or the default one if there are none
        string GetCharacterId()
        {
            BoardItems.CharactersData charactersData = Data.Instance.charactersData;

            string playerId = miniGame.PlayerCharacterId;
            if (!string.IsNullOrEmpty(playerId)) {
                if (charactersData.GetUserCharacter(playerId) != null || charactersData.GetMeta(playerId) != null)
                    return playerId;
                Debug.LogWarning("MiniGameScreen: player character not found: " + playerId);
            }

            foreach (CharacterMetaData md in charactersData.userCharactersMetaData) {
                if (charactersData.GetUserCharacter(md.id) != null)
                    return md.id;
            }
            return defaultCharacterId;
        }

        public override void OnDestroyed()
        {
            if (step != Step.Closed && stage != null)
                stage.Close();
        }
    }
}
