using System;
using System.Collections.Generic;
using System.Data;
using FishNet.Connection;
using FishNet.Object;

namespace Assets.Scripts.Modules
{
    public class MessageBoxModule : NetworkBehaviour
    {
        private static MessageBoxModule Singleton;

        private void Awake()
        {
            Singleton = this;
        }

        private Dictionary<int, Action<bool>> activePrompts = new();

        public static event Action<string> OnNotificationReceived;
        public static event Action<string> OnPromptReceived;

        public void SendMessage(NetworkConnection client, string message)
        {
            if(! IsServerInitialized) return;

;           NotifyPlayer(client, message);
        }

        public void SendPrompt(NetworkConnection client, string message, Action<bool> onPromptConfirmed)
        {
            if(! IsServerInitialized) return;

            activePrompts.Add(client.ClientId, onPromptConfirmed);

            PromptPlayer(client, message);
        }

        [TargetRpc]
        private void NotifyPlayer(NetworkConnection client, string message)
        {
            OnNotificationReceived?.Invoke(message);
        }

        [TargetRpc]
        private void PromptPlayer(NetworkConnection client, string message)
        {
            OnPromptReceived?.Invoke(message);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ConfirmPrompt(bool isConfirmed, NetworkConnection sender = null)
        {
            if (! activePrompts.ContainsKey(sender.ClientId)) return;

            Action<bool> promptResponse = activePrompts[sender.ClientId];
            promptResponse?.Invoke(isConfirmed);

            activePrompts.Remove(sender.ClientId);
        }
    }
}