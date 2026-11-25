using System;
using UnityEngine;

namespace MiniGames
{
    // Base class for every minigame. The screen gives it the stage and the character to play with
    public abstract class MiniGame : MonoBehaviour
    {
        [Tooltip("Id of the game in the web URLs (?minigame=<id>). Empty = the class name without \"MiniGame\", lowercase (RunnerMiniGame: runner)")]
        [SerializeField] string id;

        public string Id => string.IsNullOrEmpty(id) ? GetType().Name.Replace("MiniGame", "").ToLowerInvariant() : id;

        [Header("Official minigame (uploaded from the Unity editor)")]
        [Tooltip("Name of the original of Fabulab made with this inspector (click derecho > Subir como minijuego oficial)")]
        [SerializeField] string officialName;
        [Tooltip("Id of that original once uploaded: uploading again updates it. Empty = uploading makes a new one")]
        [SerializeField] string officialId;

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

        // Copies of the works it shows (MiniGameConfig.works). Null: the originals (what the inspector sets)
        protected BoardItems.BoardData.StoryAssetsData works;

        // The minigame has its own copy of this character (it can play even if the original was deleted)
        public bool HasCopyOfCharacter(string id)
        {
            return works != null && !string.IsNullOrEmpty(id) && works.characters.ContainsKey(id);
        }

        // Shows a minigame saved by a user (MiniGameConfig) instead of what the inspector sets
        public virtual void Apply(MiniGameConfig config)
        {
            playerCharacterId = config.playerCharacterId;
            introStoryId = config.introStoryId;
            outroStoryId = config.outroStoryId;
            works = config.works;
        }

        // What it shows now: the inspector values, or the last config applied. A new minigame starts from it
        public virtual MiniGameConfig GetConfig()
        {
            return new MiniGameConfig {
                mechanic = Id,
                playerCharacterId = playerCharacterId,
                introStoryId = introStoryId,
                outroStoryId = outroStoryId
            };
        }

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
            // From now on the stage shows the copies of the minigame instead of the originals
            stage.SetWorks(works);
        }

        public abstract void Play();

        // Uploads what this inspector sets as an original minigame of Fabulab (MiniGameMetaData.isOfficial), the
        // model the users copy. In Play mode, signed in with the Fabulab account, with the minigame screen open
        // (its thumbnail is what the screen shows)
        [ContextMenu("Subir como minijuego oficial")]
        void UploadAsOfficial()
        {
            if (!Application.isPlaying) {
                Report("Entrá en Play, ingresá con la cuenta de Fabulab y abrí el minijuego antes de subirlo.", false);
                return;
            }
            string uid = Data.Instance.userData.userDataInDatabase.uid;
            if (string.IsNullOrEmpty(uid) || uid != Data.Instance.adminData.GetFabulabId()) {
                Report("Hay que ingresar con la cuenta de Fabulab: los minijuegos oficiales quedan en esa cuenta.", false);
                return;
            }
            MiniGameScreen screen = FindFirstObjectByType<MiniGameScreen>(FindObjectsInactive.Include);
            Texture2D picture = screen != null ? screen.CaptureView() : null;
            if (picture == null) {
                Report("Abrí la pantalla del minijuego (se usa lo que muestra como thumbnail) y volvé a subirlo.", false);
                return;
            }
            Vector2Int size = Data.Instance.charactersData.ThumbSize;
            Texture2D thumb = size.x > 0 && size.y > 0 ? TextureUtils.GPUScaleTexture(picture, size.x, size.y) : picture;
            string name = string.IsNullOrWhiteSpace(officialName) ? Id : officialName.Trim();
            MiniGameConfig config = GetConfig();

            if (string.IsNullOrEmpty(officialId)) {
                MiniGamesData.Save(null, name, true, config, thumb, OnUploaded, official: true);
                return;
            }
            MiniGamesData.LoadMeta(officialId, meta => {
                if (meta == null || !meta.isOfficial || !MiniGamesData.IsOwn(meta)) {
                    Report("Official Id (" + officialId + ") no es un minijuego oficial de Fabulab. Vacialo para subir uno nuevo.", false);
                    return;
                }
                MiniGamesData.Save(meta, name, meta.isPublic, config, thumb, OnUploaded);
            });
        }

        void OnUploaded(MiniGameMetaData saved)
        {
            if (saved == null) {
                Report("No se pudo subir el minijuego (ver la consola).", false);
                return;
            }
            GUIUtility.systemCopyBuffer = saved.id;
            bool isNew = saved.id != officialId;
            Report("Minijuego oficial \"" + saved.name + "\" " + (isNew ? "subido" : "actualizado") + ": " + saved.id
                + (isNew ? "\n\nEl id quedó copiado: pegalo en Official Id de este componente (fuera de Play, así se guarda)"
                    + " para que las próximas subidas actualicen este mismo." : ""), true);
        }

        static void Report(string message, bool ok)
        {
            if (ok) Debug.Log("[MiniGame] " + message);
            else Debug.LogError("[MiniGame] " + message);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("Minijuego oficial", message, "OK");
#endif
        }

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
