using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Editing
{
    // Picture + name of a work (an option of the picker, an item of a list, the current value of a slot)
    [Serializable]
    public class MiniGameOptionView
    {
        public RawImage thumb;
        public TMP_Text label;
        [Tooltip("Shown when the work is of another user")]
        public GameObject othersBadge;

        int request;

        public void Show(MiniGameOption option, string emptyText = "") {
            int current = ++request;
            if (label != null)
                label.text = option == null ? emptyText : (string.IsNullOrEmpty(option.name) ? "Sin nombre" : option.name);
            if (othersBadge != null)
                othersBadge.SetActive(option != null && !option.isOwn && !string.IsNullOrEmpty(option.userID));
            if (thumb == null)
                return;
            thumb.texture = null;
            thumb.enabled = false;
            if (option == null)
                return;
            MiniGameEditorOptions.LoadThumb(option, tex => {
                // A newer value may have been shown meanwhile
                if (current != request || thumb == null) return;
                thumb.texture = tex;
                thumb.enabled = tex != null;
            });
        }
    }
}
