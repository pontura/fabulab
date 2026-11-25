using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Editing
{
    // A list of the minigame: its items (instances of itemPrefab) and a button to add one
    public class MiniGameEditorList : MonoBehaviour
    {
        [Tooltip("Required: where the items are placed")]
        [SerializeField] Transform content;
        [Tooltip("Required")]
        [SerializeField] MiniGameEditorListItem itemPrefab;
        [Tooltip("Required: opens the picker to add one")]
        [SerializeField] Button addButton;

        public event Action OnAdd;
        readonly List<MiniGameEditorListItem> items = new List<MiniGameEditorListItem>();

        void Awake() {
            if (addButton != null) addButton.onClick.AddListener(() => OnAdd?.Invoke());
            if (itemPrefab != null) itemPrefab.gameObject.SetActive(false);
        }

        // Rebuilds the items. setup is called with each new item and its index (to subscribe its events)
        public void Show(int count, Action<MiniGameEditorListItem, int> setup) {
            foreach (MiniGameEditorListItem item in items)
                Destroy(item.gameObject);
            items.Clear();
            for (int i = 0; i < count; i++) {
                MiniGameEditorListItem item = Instantiate(itemPrefab, content);
                item.gameObject.SetActive(true);
                items.Add(item);
                setup(item, i);
            }
        }
    }
}
