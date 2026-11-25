using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Editing
{
    // Panel to choose a work: the own ones first, then the public ones of other users
    public class MiniGameEditorPicker : MonoBehaviour
    {
        [Tooltip("Required: shown while choosing")]
        [SerializeField] GameObject panel;
        [Tooltip("Required: where the options are placed")]
        [SerializeField] Transform content;
        [Tooltip("Required")]
        [SerializeField] MiniGameEditorOptionButton optionPrefab;
        [SerializeField] TMP_Text title;
        [SerializeField] Button closeButton;
        [Tooltip("Shown when there is nothing to choose")]
        [SerializeField] GameObject emptyMessage;

        readonly List<MiniGameEditorOptionButton> options = new List<MiniGameEditorOptionButton>();

        void Awake() {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (optionPrefab != null) optionPrefab.gameObject.SetActive(false);
            panel.SetActive(false);
        }

        public void Open(MiniGameOptionKind kind, string titleText, Action<MiniGameOption> onPick) {
            if (title != null) title.text = titleText;
            foreach (MiniGameEditorOptionButton o in options)
                Destroy(o.gameObject);
            options.Clear();

            List<MiniGameOption> list = MiniGameEditorOptions.Get(kind);
            foreach (MiniGameOption option in list) {
                MiniGameEditorOptionButton button = Instantiate(optionPrefab, content);
                button.gameObject.SetActive(true);
                button.Show(option, () => {
                    Close();
                    onPick(option);
                });
                options.Add(button);
            }
            if (emptyMessage != null) emptyMessage.SetActive(list.Count == 0);
            panel.SetActive(true);
        }

        public void Close() {
            panel.SetActive(false);
        }

        public bool IsOpen => panel.activeSelf;
    }
}
