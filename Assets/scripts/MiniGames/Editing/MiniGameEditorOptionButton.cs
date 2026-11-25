using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Editing
{
    // An option of the picker
    public class MiniGameEditorOptionButton : MonoBehaviour
    {
        [SerializeField] MiniGameOptionView view = new MiniGameOptionView();
        [Tooltip("Required")]
        [SerializeField] Button button;

        public void Show(MiniGameOption option, Action onClick) {
            view.Show(option);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick());
        }
    }
}
