using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Editing
{
    // An item of a list of the minigame (an obstacle, an object of the parallax)
    public class MiniGameEditorListItem : MonoBehaviour
    {
        [SerializeField] MiniGameOptionView view = new MiniGameOptionView();
        [SerializeField] Button removeButton;
        [Tooltip("Obstacles only: the action the character does")]
        [SerializeField] TMP_Dropdown actionDropdown;
        [Tooltip("Parallax only: moves it one place before")]
        [SerializeField] Button moveUpButton;

        public event Action OnRemove;
        public event Action OnMoveUp;
        public event Action<CharacterAction> OnActionChanged;

        void Awake() {
            if (removeButton != null) removeButton.onClick.AddListener(() => OnRemove?.Invoke());
            if (moveUpButton != null) moveUpButton.onClick.AddListener(() => OnMoveUp?.Invoke());
            if (actionDropdown != null) {
                actionDropdown.ClearOptions();
                actionDropdown.AddOptions(ActionNames());
                actionDropdown.onValueChanged.AddListener(i => OnActionChanged?.Invoke((CharacterAction)i));
            }
        }

        public void Show(MiniGameOption option, CharacterAction? action) {
            view.Show(option);
            if (actionDropdown != null) {
                actionDropdown.gameObject.SetActive(action.HasValue);
                if (action.HasValue)
                    actionDropdown.SetValueWithoutNotify((int)action.Value);
            }
        }

        // The names the app shows (InspectorName of CharacterAction)
        static List<string> ActionNames() {
            List<string> names = new List<string>();
            foreach (CharacterAction a in Enum.GetValues(typeof(CharacterAction))) {
                InspectorNameAttribute attr = typeof(CharacterAction).GetField(a.ToString()).GetCustomAttribute<InspectorNameAttribute>();
                names.Add(attr != null ? attr.displayName : a.ToString());
            }
            return names;
        }
    }
}
