using System.Collections.Generic;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.UI.Controllers;
using FishNet;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class PartyController : MonoBehaviour
    {
        public static PartyController Singleton { get; private set; }

        private PlayerCharacter selectedPlayer;

        void OnEnable()
        {
            Singleton = this;

            PartyServerController.OnInviteReceived += InviteReceived;
            PartyServerController.OnPlayerJoined += OnPlayerJoined;
            PartyServerController.OnPlayerLeft += OnPlayerLeft;
            PartyServerController.OnPlayerKick += OnPlayerKick;
        }

        void OnDisable()
        {
            PartyServerController.OnInviteReceived -= InviteReceived;
            PartyServerController.OnPlayerJoined -= OnPlayerJoined;
            PartyServerController.OnPlayerLeft -= OnPlayerLeft;
            PartyServerController.OnPlayerKick -= OnPlayerKick;
        }

        private IParty CreateParty()
        {
            PlayerCharacter playerCharacter = PlayerController.Singleton.GetPlayerCharacter();
            int clientId = PlayerController.Singleton.OwnerId;

            HUDController.Singleton.ShowLeavePartyButton();

            IParty party = new Party(playerCharacter, clientId);
            PlayerController.Singleton.SetParty(party);

            return party;
        }

        private void DestroyParty()
        {
            PlayerController.Singleton.SetParty(null);
            HUDController.Singleton.HideLeavePartyButton();
        }

        public void OnPartyJoin(Dictionary<int, PlayerCharacter> members)
        {
            IParty party = new Party(-1, members);
            PlayerController.Singleton.SetParty(party);

            HUDController.Singleton.UpdateParty(party);

            HUDController.Singleton.ShowLeavePartyButton();

            HUDController.Singleton.ShowMessage("You have joined the party!");
        }

        public void OnPartyLeave()
        {
            LeaveParty("You have left the party!");
        }

        public void OnPartyKick()
        {
            LeaveParty("You have been kicked from the party!");
        }

        private void LeaveParty(string reason)
        {
            DestroyParty();

            HUDController.Singleton.ClearParty();

            HUDController.Singleton.ShowMessage(reason);
        }

        public void ShowInvitePlayerPrompt(PlayerCharacter invitedPlayer)
        {
            this.selectedPlayer = invitedPlayer;
            HUDController.Singleton.ShowPrompt("Do you really wish to invite " + invitedPlayer.GetName() + " to your party?", HandlePlayerInvitePrompt);
        }

        public void HandlePlayerInvitePrompt(bool isConfirmed)
        {
            if (!isConfirmed) return;

            InvitePlayerButton();
        }

        public void InvitePlayerButton()
        {
            PartyServerController.Singleton.InvitePlayer(this.selectedPlayer);
        }

        public void InviteReceived(string from)
        {
            HUDController.Singleton.ShowPrompt(from + " has invited you to join their party. Do you want to accept?", (hasAccepted) =>
            {
                if (hasAccepted)
                {
                    AcceptInviteButton();
                    return;
                }

                DeclineInviteButton();
            });
        }

        private void AcceptInviteButton()
        {
            PartyServerController.Singleton.AcceptInvite();
        }

        private void DeclineInviteButton()
        {
            PartyServerController.Singleton.DeclineInvite();
        }

        public void ShowLeavePartyPrompt()
        {
            HUDController.Singleton.ShowPrompt("Do you really wish to leave your party?", HandlePartyLeavePrompt);
        }

        public void HandlePartyLeavePrompt(bool isConfirmed)
        {
            if (!isConfirmed) return;

            LeavePartyButton();
        }

        private void LeavePartyButton()
        {
            PartyServerController.Singleton.LeaveParty();
        }

        public void ShowKickPlayerPrompt(PlayerCharacter kickedPlayer)
        {
            selectedPlayer = kickedPlayer;
            HUDController.Singleton.ShowPrompt("Do you really wish to kick " + kickedPlayer.GetName() + " from your party?", HandlePlayerKickPrompt);
        }

        public void HandlePlayerKickPrompt(bool isKicked)
        {
            if (!isKicked) return;

            KickPlayerButton();
        }

        private void KickPlayerButton()
        {
            PartyServerController.Singleton.KickPlayer(selectedPlayer);
        }

        private void OnPlayerJoined(PlayerCharacter player, int clientId)
        {
            IParty party = PlayerController.Singleton.GetParty();

            party ??= CreateParty();

            party.AddMember(clientId, player);

            HUDController.Singleton.AddPlayerToParty(player);

            HUDController.Singleton.ShowMessage(player.GetName() + "has joined the party!");
        }

        private void OnPlayerLeft(uint playerId, int clientId, bool isNewLeader)
        {
            IParty party = PlayerController.Singleton.GetParty();

            if (party == null) return;

            PlayerCharacter player = party.GetMemberById(playerId);

            RemovePlayer(party, playerId, clientId);

            if (party.GetMemberCount() <= 1)
            {
                DestroyParty();
                isNewLeader = false;
            }

            if (isNewLeader)
            {
                int myId = InstanceFinder.ClientManager.Connection.ClientId;
                party.ChangeLeader(myId);
            }

            if (player == null) return;

            HUDController.Singleton.ShowMessage(player.GetName() + " has left the party!" + (isNewLeader ? " You have been chosen as the new party leader." : ""));
        }

        private void OnPlayerKick(uint playerId, int clientId)
        {
            IParty party = PlayerController.Singleton.GetParty();

            if (party == null) return;

            PlayerCharacter player = party.GetMemberById(playerId);

            RemovePlayer(party, playerId, clientId);

            if (party.GetMemberCount() <= 1)
            {
                DestroyParty();
            }

            if (player == null) return;

            HUDController.Singleton.ShowMessage(player.GetName() + " has been kicked from the party!");
        }

        private void RemovePlayer(IParty party, uint playerId, int clientId)
        {
            if (party == null) return;

            party.RemoveMember(clientId);

            HUDController.Singleton.RemovePlayerFromParty(playerId);
        }
    }
}
