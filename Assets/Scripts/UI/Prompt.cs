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
        [SerializeField] private Button declineButton;

        public void SetupPrompt(string message, Action<bool> onConfirm)
        {
            DisplayMessage(message);
            RegisterEvents(onConfirm);
        }

        private void RegisterEvents(Action<bool> onConfirm)
        {
            confirmButton.onClick.AddListener(() =>
            {
                confirmButton.onClick.RemoveAllListeners();
                CloseWindow();

                onConfirm?.Invoke(true);
            });

            declineButton.onClick.AddListener(() =>
            {
                declineButton.onClick.RemoveAllListeners();
                CloseWindow();

                onConfirm?.Invoke(false);
            });
        }
    }
}
