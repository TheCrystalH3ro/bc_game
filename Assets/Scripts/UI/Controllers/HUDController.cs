using System;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using UnityEngine;

namespace Assets.Scripts.UI.Controllers
{
    public class HUDController : MonoBehaviour
    {
        public static HUDController Singleton { get; private set; }

        public PauseMenu PauseMenu;
        public PlayerCardController PlayerCard;
        public PartyStatus PartyStatus;
        public GameObject partyLeaveButton;

        [SerializeField] private MessageBox messageBox;
        [SerializeField] private Prompt prompt;
        [SerializeField] private GameObject loadingScreen;

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

        public void UpdateParty(IParty party)
        {
            PartyStatus.Init(party.GetMembers());
        }

        public void ClearParty()
        {
            PartyStatus.ClearPlayers();
        }

        public void AddPlayerToParty(PlayerCharacter player)
        {
            PartyStatus.AddPlayer(player);
        }

        public void RemovePlayerFromParty(uint playerId)
        {
            PartyStatus.RemovePlayer(playerId);
        }

        public void PartyChangedZone(uint characterId, bool isInSameZone)
        {
            if (isInSameZone)
            {
                PartyStatus.EnteredZone(characterId);
                return;
            }

            PartyStatus.LeftZone(characterId);
        }

        public void ShowLeavePartyButton()
        {
            partyLeaveButton.SetActive(true);
        }

        public void HideLeavePartyButton()
        {
            partyLeaveButton.SetActive(false);
        }

        public void ShowLoadingScreen()
        {
            Debug.Log("Opening loading screen");
            loadingScreen.SetActive(true);
        }

        public void HideLoadingScreen()
        {
            loadingScreen.SetActive(false);
        }
    }
}