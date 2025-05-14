using System;
using System.Collections;
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
        }

        public void ShowMessage(string message)
        {
            messageBox.DisplayMessage(message);
        }

        public void ShowPrompt(string message, Action onConfirm)
        {
            prompt.SetupPrompt(message, onConfirm);
        }
    }
}