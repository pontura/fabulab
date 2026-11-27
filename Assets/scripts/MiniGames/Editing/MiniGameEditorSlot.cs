using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Editing
{
    // One value of the minigame (background, main character, intro story, final story): shows it, and its button
    // opens the picker. Its second button either leaves it empty (ex: the main character) or, with UseCreate, makes a
    // new work for it (the stories and the background, that cannot be left empty).
    // Part of the minigame editor (MiniGameEditorScreen), to be assembled in a prefab
    public class MiniGameEditorSlot : MonoBehaviour
    {
        [SerializeField] MiniGameOptionView view = new MiniGameOptionView();
        [Tooltip("Required: opens the picker")]
        [SerializeField] Button pickButton;
        [Tooltip("Leaves it empty (ex: no main character). With UseCreate it is the button to create a new one")]
        [SerializeField] Button clearButton;
        [Tooltip("Opens the work in its editor (shown when there is one)")]
        [SerializeField] Button editButton;
        [SerializeField] string emptyText = "Elegir";
        [SerializeField] string createText = "CREAR";

        bool createMode;
        bool noClear;

        public event Action OnPick;
        public event Action OnClear;
        public event Action OnCreate;
        public event Action OnEdit;

        void Awake() {
            if (pickButton != null) pickButton.onClick.AddListener(() => OnPick?.Invoke());
            if (editButton != null) editButton.onClick.AddListener(() => OnEdit?.Invoke());
            if (clearButton != null) clearButton.onClick.AddListener(() => {
                if (createMode) OnCreate?.Invoke();
                else OnClear?.Invoke();
            });
        }

        // The second button creates a new work instead of leaving it empty, and is always shown
        public void UseCreate() {
            createMode = true;
            if (clearButton == null) return;
            TMP_Text label = clearButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = createText;
            clearButton.gameObject.SetActive(true);
        }

        // Without the second button: it can only be changed for another one
        public void UseNoClear() {
            noClear = true;
            if (clearButton != null) clearButton.gameObject.SetActive(false);
        }

        public void Show(MiniGameOption option) {
            view.Show(option, emptyText);
            if (clearButton != null)
                clearButton.gameObject.SetActive(createMode || (!noClear && option != null));
            if (editButton != null)
                editButton.gameObject.SetActive(option != null);
        }
    }
}
