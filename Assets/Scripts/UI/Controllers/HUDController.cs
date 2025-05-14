using System;
using System.Collections;
using Assets.Scripts.Modules;
using UnityEngine;

namespace Assets.Scripts.UI.Controllers
{
    public class HUDController : MonoBehaviour
    {
        public static HUDController Singleton { get; private set; }

        public PauseMenu PauseMenu;
        public PlayerCardController PlayerCard;

        [SerializeField] private MessageBox messageBox;
        [SerializeField] private Prompt prompt;

        void OnEnable()
        {
            Singleton = FindFirstObjectByType<HUDController>();

            MessageBoxModule.OnNotificationReceived += OnMessageReceived;
            MessageBoxModule.OnPromptReceived += OnPromptReceived;
        }

        private void OnDisable()
        {
            MessageBoxModule.OnNotificationReceived -= OnMessageReceived;
            MessageBoxModule.OnPromptReceived -= OnPromptReceived;
        }

        public void ShowMessage(string message)
        {
            messageBox.DisplayMessage(message);
        }

        public void ShowPrompt(string message, Action<bool> onConfirm)
        {
            prompt.SetupPrompt(message, onConfirm);
        }

        public void OnMessageReceived(string message)
        {
            ShowMessage(message);
        }

        public void OnPromptReceived(string message)
        {
            ShowPrompt(message, (isConfirmed) =>
            {
                MessageBoxModule.Singleton.ConfirmPrompt(isConfirmed);
            });
        }
    }
}