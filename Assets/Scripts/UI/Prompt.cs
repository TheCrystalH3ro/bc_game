using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class Prompt : MessageBox
    {
        [SerializeField] private Button confirmButton;

        public void SetupPrompt(string message, Action onConfirm)
        {
            DisplayMessage(message);
            RegisterEvents(onConfirm);
        }

        private void RegisterEvents(Action onConfirm)
        {
            confirmButton.onClick.AddListener(() =>
            {
                confirmButton.onClick.RemoveAllListeners();
                CloseWindow();

                onConfirm?.Invoke();
            });
        }
    }
}
