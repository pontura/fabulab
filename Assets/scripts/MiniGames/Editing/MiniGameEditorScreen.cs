using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UI.MainApp;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Editing
{
    // Screen to edit a minigame (UIManager.screenType.MiniGameEditor): its name, background, main character,
    // obstacles (characters doing an action), objects of the parallax, intro story and final story, with a preview
    // where it can be played and a button to take its thumbnail from it.
    // Saving a minigame of another user makes a copy in the account of the user (MiniGamesData.Save).
    // Opened by WebEditorController (?type=minigame&id=<id>). The views are optional: the prefab assigns the ones it has
    public class MiniGameEditorScreen : MainScreen
    {
        [Tooltip("Gives the game and the stage of the preview. Empty: the one of the scene")]
        [SerializeField] MiniGameScreen miniGameScreen;

        [Header("Preview")]
        [Tooltip("Shows the game. Required")]
        [SerializeField] RawImage preview;
        [Tooltip("Plays it again from the start")]
        [SerializeField] Button restartButton;
        [Tooltip("Seconds after losing before the preview starts again")]
        [SerializeField] float restartDelay = 1.5f;

        [Header("Name and thumbnail")]
        [SerializeField] TMP_InputField nameField;
        [SerializeField] Toggle publicToggle;
        [Tooltip("Takes the thumbnail from the preview")]
        [SerializeField] Button thumbnailButton;
        [Tooltip("Shows the thumbnail")]
        [SerializeField] RawImage thumbnailView;
        [SerializeField] string defaultName = "Mi juego";

        [Header("Parts")]
        [SerializeField] MiniGameEditorSlot backgroundSlot;
        [SerializeField] MiniGameEditorSlot playerSlot;
        [SerializeField] MiniGameEditorList obstaclesList;
        [SerializeField] MiniGameEditorList parallaxList;
        [SerializeField] MiniGameEditorSlot introSlot;
        [SerializeField] MiniGameEditorSlot outroSlot;
        [Tooltip("Required if there are slots or lists")]
        [SerializeField] MiniGameEditorPicker picker;

        [Header("Save")]
        [SerializeField] Button saveButton;
        [Tooltip("Shown when the minigame is of another user: saving makes a copy")]
        [SerializeField] GameObject copyNotice;

        // The minigame being edited (the originals of Fabulab are uploaded from the Unity editor, see MiniGame)
        MiniGameMetaData meta;
        MiniGameConfig config;
        Texture2D newThumb;
        bool isOpen;
        bool saving;
        Coroutine restarting;
        Coroutine pendingPreview;

        public event Action<MiniGameMetaData> OnSaved;
        public event Action<string> OnFailed;

        public MiniGameConfig Config => config;
        public bool IsCopy => meta != null && !MiniGamesData.IsOwn(meta);

        MiniGameScreen GameScreen {
            get {
                if (miniGameScreen == null)
                    miniGameScreen = FindFirstObjectByType<MiniGameScreen>(FindObjectsInactive.Include);
                return miniGameScreen;
            }
        }
        MiniGame Game => GameScreen != null ? GameScreen.Game : null;
        MiniGameStage Stage => GameScreen != null ? GameScreen.Stage : null;

        void Start() {
            if (restartButton != null) restartButton.onClick.AddListener(RestartPreview);
            if (thumbnailButton != null) thumbnailButton.onClick.AddListener(TakeThumbnail);
            if (saveButton != null) saveButton.onClick.AddListener(Save);
            if (nameField != null) nameField.onValueChanged.AddListener(_ => MarkChanged());
            if (publicToggle != null) publicToggle.onValueChanged.AddListener(_ => MarkChanged());

            // The background and the stories cannot be left empty: their second button creates a new one
            if (backgroundSlot != null) {
                backgroundSlot.UseCreate();
                backgroundSlot.OnPick += () => Pick(MiniGameOptionKind.Background, "Fondo", o => { config.backgroundId = o.id; Changed(true); });
                backgroundSlot.OnCreate += () => Create(MiniGameOptionKind.Background, id => { config.backgroundId = id; return true; });
            }
            // The main character cannot be left empty either: only changed for another one
            if (playerSlot != null) {
                playerSlot.UseNoClear();
                playerSlot.OnPick += () => Pick(MiniGameOptionKind.Character, "Personaje principal", o => { config.playerCharacterId = o.id; Changed(true); });
            }
            if (introSlot != null) {
                introSlot.UseCreate();
                introSlot.OnPick += () => Pick(MiniGameOptionKind.Story, "Historia de intro", o => { config.introStoryId = o.id; Changed(false); });
                introSlot.OnCreate += () => Create(MiniGameOptionKind.Story, id => { config.introStoryId = id; return false; });
            }
            if (outroSlot != null) {
                outroSlot.UseCreate();
                outroSlot.OnPick += () => Pick(MiniGameOptionKind.Story, "Historia de game over", o => { config.outroStoryId = o.id; Changed(false); });
                outroSlot.OnCreate += () => Create(MiniGameOptionKind.Story, id => { config.outroStoryId = id; return false; });
            }
            if (obstaclesList != null)
                obstaclesList.OnAdd += () => Pick(MiniGameOptionKind.Character, "Obstáculo", o => {
                    config.obstacles.Add(new MiniGameObstacle { characterId = o.id });
                    Changed(true);
                });
            if (parallaxList != null)
                parallaxList.OnAdd += () => Pick(MiniGameOptionKind.Object, "Objeto del fondo", o => {
                    config.parallaxObjectIds.Add(o.id);
                    Changed(true);
                });
        }

        // --- Opening ---

        // Opens a minigame saved by a user. onDone(false) when it does not exist
        public void Open(string id, Action<bool> onDone = null) {
            Events.OnLoading(true);
            MiniGamesData.Load(id, (loadedMeta, loadedConfig) => {
                Events.OnLoading(false);
                if (loadedMeta == null || loadedConfig == null) {
                    onDone?.Invoke(false);
                    return;
                }
                MiniGamesData.LoadThumb(loadedMeta, tex => {
                    loadedMeta.thumb = tex;
                    if (meta == loadedMeta) ShowThumbnail();
                });
                Begin(loadedMeta, loadedConfig);
                onDone?.Invoke(true);
            });
        }

        void Begin(MiniGameMetaData openedMeta, MiniGameConfig openedConfig) {
            meta = openedMeta;
            config = openedConfig.Clone();
            if (string.IsNullOrEmpty(config.mechanic) && Game != null)
                config.mechanic = Game.Id;
            newThumb = null;
            if (nameField != null) nameField.SetTextWithoutNotify(meta != null && !string.IsNullOrEmpty(meta.name) ? meta.name : defaultName);
            if (publicToggle != null) publicToggle.SetIsOnWithoutNotify(meta == null || meta.isPublic);
            if (copyNotice != null) copyNotice.SetActive(IsCopy);
            UIManager.Instance.boardUI.SetEditingType(BoardUI.editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.MiniGameEditor);
            UIManager.Instance.hasUnsavedChanges = false;
        }

        protected override void ShowScreen(UIManager.screenType type) {
            // The editor of the work being created was left (saved or not): back here
            if (type == UIManager.screenType.Home && creating != null) {
                StartCoroutine(ReturnFromCreating());
                return;
            }
            if (type == UIManager.screenType.MiniGameEditor) {
                if (!isOpen && config != null)
                    Show(true);
            } else if (isOpen) {
                Close();
            }
        }

        public override void OnInit() {
            isOpen = true;
            if (Game == null || Stage == null) {
                Debug.LogError("[MiniGameEditor] The scene has no MiniGameScreen with a game and a stage");
                return;
            }
            Game.OnFinished += OnPreviewFinished;
            Stage.Open(preview);
            RefreshAll();
            StartPreview();
        }

        void Close() {
            isOpen = false;
            StopAllCoroutines();
            restarting = null;
            pendingPreview = null;
            if (picker != null) picker.Close();
            if (Game != null) {
                Game.OnFinished -= OnPreviewFinished;
                Game.Stop();
            }
            if (Stage != null) Stage.Close();
            Show(false);
        }

        public override void OnDestroyed() {
            if (isOpen && Stage != null)
                Stage.Close();
        }

        // --- Editing ---

        void Pick(MiniGameOptionKind kind, string title, Action<MiniGameOption> onPick) {
            if (picker != null)
                picker.Open(kind, title, onPick);
        }

        // --- Creating a story or a background for the minigame ---

        // Assigns the work created to the minigame. Returns whether the preview shows it
        Func<string, bool> creating;
        // The minigame had changes not saved before going to create
        bool changedBeforeCreating;

        // Opens the editor of a new story or background, that gets the tag "minigame". Leaving it comes back here
        void Create(MiniGameOptionKind kind, Func<string, bool> assign) {
            if (creating != null) return;
            Events.OnLoading(true);
            MiniGameCreation.GetTagId(tagId => {
                Events.OnLoading(false);
                if (string.IsNullOrEmpty(tagId)) {
                    Events.OnPopupTopSignalText("No se pudo crear");
                    return;
                }
                creating = assign;
                changedBeforeCreating = UIManager.Instance.hasUnsavedChanges;
                MiniGameCreation.Begin(kind, tagId);
                UIManager.Instance.CreateSelected(kind == MiniGameOptionKind.Story ? 1 : 4, true);
            });
        }

        // The other listeners of the event are still showing the home
        IEnumerator ReturnFromCreating() {
            yield return null;
            string createdId = MiniGameCreation.CreatedId;
            Func<string, bool> assign = creating;
            creating = null;
            MiniGameCreation.End();
            UIManager.Instance.boardUI.SetEditingType(BoardUI.editingTypes.NONE);
            Events.ShowScreen(UIManager.screenType.MiniGameEditor);
            UIManager.Instance.hasUnsavedChanges = changedBeforeCreating;
            if (!string.IsNullOrEmpty(createdId))
                Changed(assign(createdId));
        }

        // affectsPreview: the game shows it (the stories are only played in the real game)
        void Changed(bool affectsPreview) {
            MarkChanged();
            RefreshAll();
            if (affectsPreview)
                SchedulePreview();
        }

        void MarkChanged() {
            UIManager.Instance.hasUnsavedChanges = true;
        }

        void RefreshAll() {
            if (config == null) return;
            if (backgroundSlot != null) backgroundSlot.Show(MiniGameEditorOptions.Find(MiniGameOptionKind.Background, config.backgroundId));
            if (playerSlot != null) playerSlot.Show(MiniGameEditorOptions.Find(MiniGameOptionKind.Character, config.playerCharacterId));
            if (introSlot != null) introSlot.Show(MiniGameEditorOptions.Find(MiniGameOptionKind.Story, config.introStoryId));
            if (outroSlot != null) outroSlot.Show(MiniGameEditorOptions.Find(MiniGameOptionKind.Story, config.outroStoryId));

            if (obstaclesList != null)
                obstaclesList.Show(config.obstacles.Count, (item, i) => {
                    MiniGameObstacle obstacle = config.obstacles[i];
                    item.Show(MiniGameEditorOptions.Find(MiniGameOptionKind.Character, obstacle.characterId), obstacle.action);
                    item.OnRemove += () => { config.obstacles.RemoveAt(i); Changed(true); };
                    item.OnActionChanged += action => { obstacle.action = action; MarkChanged(); SchedulePreview(); };
                });
            if (parallaxList != null)
                parallaxList.Show(config.parallaxObjectIds.Count, (item, i) => {
                    item.Show(MiniGameEditorOptions.Find(MiniGameOptionKind.Object, config.parallaxObjectIds[i]), null);
                    item.OnRemove += () => { config.parallaxObjectIds.RemoveAt(i); Changed(true); };
                    item.OnMoveUp += () => {
                        if (i == 0) return;
                        (config.parallaxObjectIds[i - 1], config.parallaxObjectIds[i]) = (config.parallaxObjectIds[i], config.parallaxObjectIds[i - 1]);
                        Changed(true);
                    };
                });
            ShowThumbnail();
        }

        // --- Preview ---

        // Several changes in a row restart the preview once
        void SchedulePreview() {
            if (!isOpen || pendingPreview != null) return;
            pendingPreview = StartCoroutine(PreviewNextFrame());
        }

        IEnumerator PreviewNextFrame() {
            yield return null;
            pendingPreview = null;
            StartPreview();
        }

        public void RestartPreview() {
            if (isOpen) StartPreview();
        }

        void StartPreview() {
            if (restarting != null) {
                StopCoroutine(restarting);
                restarting = null;
            }
            Game.Stop();
            Stage.Clear();
            Stage.ResetBackgroundColor();
            Stage.SetRendering(true);
            Game.Apply(config);
            Game.Init(Stage, GameScreen.GetCharacterId());
            Game.Play();
        }

        void OnPreviewFinished(int score) {
            if (isOpen)
                restarting = StartCoroutine(RestartAfterDelay());
        }

        IEnumerator RestartAfterDelay() {
            yield return new WaitForSeconds(restartDelay);
            restarting = null;
            StartPreview();
        }

        // --- Thumbnail ---

        // Takes the thumbnail from what the preview shows now
        public void TakeThumbnail() {
            Texture2D tex = Capture();
            if (tex == null) return;
            newThumb = tex;
            MarkChanged();
            ShowThumbnail();
        }

        Texture2D Capture() {
            RenderTexture rt = preview != null ? preview.texture as RenderTexture : null;
            if (rt == null) {
                Debug.LogWarning("[MiniGameEditor] Nothing to take the thumbnail from");
                return null;
            }
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D full = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            full.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            full.Apply();
            RenderTexture.active = previous;
            Vector2Int size = Data.Instance.charactersData.ThumbSize;
            if (size.x <= 0 || size.y <= 0)
                return full;
            Texture2D thumb = TextureUtils.GPUScaleTexture(full, size.x, size.y);
            Destroy(full);
            return thumb;
        }

        void ShowThumbnail() {
            if (thumbnailView == null) return;
            Texture2D tex = newThumb != null ? newThumb : meta?.thumb;
            thumbnailView.texture = tex;
            thumbnailView.enabled = tex != null;
        }

        // --- Saving ---

        public void Save() {
            if (saving || config == null) return;
            // A new minigame or a copy needs its own thumbnail: without one, it is taken from the preview now
            if (newThumb == null && (meta == null || IsCopy))
                newThumb = Capture();

            string name = nameField != null && !string.IsNullOrWhiteSpace(nameField.text) ? nameField.text.Trim() : defaultName;
            bool isPublic = publicToggle == null || publicToggle.isOn;
            saving = true;
            Events.OnLoading(true);
            MiniGamesData.Save(meta, name, isPublic, config, newThumb, saved => {
                saving = false;
                Events.OnLoading(false);
                if (saved == null) {
                    OnFailed?.Invoke("No se pudo guardar el juego");
                    Events.OnPopupTopSignalText("No se pudo guardar el juego");
                    return;
                }
                meta = saved;
                newThumb = null;
                UIManager.Instance.hasUnsavedChanges = false;
                Events.OnPopupTopSignalText("Juego guardado");
                OnSaved?.Invoke(saved);
                // Like the other editors: once saved, back to the home (the web editor closes)
                UIManager.Instance.Home();
            });
        }
    }
}
