using System;
using UnityEngine;

namespace MiniGames
{
    // Base class for every minigame. The screen gives it the stage and the character to play with
    public abstract class MiniGame : MonoBehaviour
    {
        [Header("Presentation (ids of the user's content, empty = skipped)")]
        [Tooltip("Character that plays the game. Empty = the first character of the user")]
        [SerializeField] string playerCharacterId;
        [Tooltip("Story played before the game. Its first frame is the game poster")]
        [SerializeField] string introStoryId;
        [Tooltip("Story played after losing")]
        [SerializeField] string outroStoryId;

        public string PlayerCharacterId => playerCharacterId;
        public string IntroStoryId => introStoryId;
        public string OutroStoryId => outroStoryId;

        protected MiniGameStage stage;
        protected string characterId;

        public bool IsPlaying { get; protected set; }
        public int Score { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action<int> OnFinished;

        public virtual void Init(MiniGameStage stage, string characterId)
        {
            this.stage = stage;
            this.characterId = characterId;
        }

        public abstract void Play();

        public virtual void Stop()
        {
            IsPlaying = false;
        }

        protected void ResetScore()
        {
            Score = 0;
            OnScoreChanged?.Invoke(Score);
        }

        protected void SetScore(int score)
        {
            if (score == Score) return;
            Score = score;
            OnScoreChanged?.Invoke(Score);
        }

        protected void Finish()
        {
            Stop();
            OnFinished?.Invoke(Score);
        }
    }
}
