using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using Assets.Scripts.Util;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers.Server
{
    public class PartyServerController : NetworkBehaviour
    {
        private static PartyServerController _instance;

        public static PartyServerController Singleton
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PartyServerController>();
                }

                if (_instance == null)
                {
                    if (!InstanceFinder.NetworkManager.IsServerStarted) return null;

                    GameObject controller = new();

                    _instance = controller.AddComponent<PartyServerController>();

                    InstanceFinder.ServerManager.Spawn(controller);
                }

                return _instance;
            }
        }

        public static readonly int MAX_PARTY_MEMBERS = 4;

        public static event Action<string> OnInviteReceived;

        public static event Action<PlayerCharacter, int> OnPlayerJoined;
        public static event Action<uint, int, bool> OnPlayerLeft;
        public static event Action<uint, int> OnPlayerKick;

        [ServerRpc(RequireOwnership = false)]
        public void InvitePlayer(PlayerCharacter invitedPlayer, NetworkConnection client = null)
        {
            PartyModule.Singleton.InvitePlayer(invitedPlayer, OnInvitePlayerSuccess, OnInvitePlayerFail, client);
        }

        private void OnInvitePlayerSuccess(NetworkConnection invitedClient, NetworkConnection invitedFromClient, PlayerCharacter invitedFrom)
        {
            MessageBoxModule.Singleton.SendMessage(invitedFromClient, "Player invitation sent.");
            NotifyPlayersInvite(invitedClient, invitedFrom);
        }

        private void OnInvitePlayerFail(NetworkConnection invitedFromClient, PlayerCharacter invitedPlayer, string errorMessage)
        {
            Debug.Log("Invite failed. Reason: " + errorMessage);
            MessageBoxModule.Singleton.SendMessage(invitedFromClient, errorMessage);
        }

        [TargetRpc]
        private void NotifyPlayersInvite(NetworkConnection invitedClient, PlayerCharacter invitedFrom)
        {
            OnInviteReceived?.Invoke(invitedFrom.GetName());
        }

        [ServerRpc(RequireOwnership = false)]
        public void AcceptInvite(NetworkConnection client = null)
        {
            PartyModule.Singleton.AcceptInvite(OnPartyAcceptSuccess, OnPartyAcceptFail, client);
        }

        private void OnPartyAcceptSuccess(NetworkConnection invitedClient, NetworkConnection partyLeaderClient, PlayerCharacter invitedPlayer, IParty party)
        {
            NotifyPlayerJoin(invitedClient, party.GetPlayers());

            List<NetworkConnection> formerMembers = party.GetConnections().Where(conn => conn != invitedClient).ToList();

            ConnectionModule.Singleton.GroupRpc(formerMembers, (Action<NetworkConnection, PlayerCharacter, int>)NotifyPartyOnPlayerJoin, invitedPlayer, invitedClient.ClientId);
        }

        private void OnPartyAcceptFail(NetworkConnection invitedClient, string errorMessage)
        {
            MessageBoxModule.Singleton.SendMessage(invitedClient, errorMessage);
        }

        [TargetRpc]
        private void NotifyPlayerJoin(NetworkConnection invitedClient, Dictionary<int, PlayerCharacter> partyMembers)
        {
            PartyController.Singleton.OnPartyJoin(partyMembers);
        }

        [TargetRpc]
        private void NotifyPartyOnPlayerJoin(NetworkConnection partyClient, PlayerCharacter invitedPlayer, int invitedClientId)
        {
            OnPlayerJoined?.Invoke(invitedPlayer, invitedClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeclineInvite(NetworkConnection client = null)
        {
            PartyModule.Singleton.DeclineInvite(OnPartyDeclineSuccess, OnPartyDeclineFail, client);
        }

        private void OnPartyDeclineSuccess(NetworkConnection invitedClient, NetworkConnection partyLeaderClient)
        {
            PlayerController partyLeaderController = ObjectUtil.FindFirstByType<PlayerController>(partyLeaderClient.Objects);
            PlayerController invitedPlayerController = ObjectUtil.FindFirstByType<PlayerController>(invitedClient.Objects);

            PlayerCharacter invitedBy = partyLeaderController.GetPlayerCharacter();
            PlayerCharacter invitedPlayer = invitedPlayerController.GetPlayerCharacter();

            IParty party = partyLeaderController.GetParty();

            //Notify leader
            if (party == null || party.IsLeader(invitedBy))
            {
                MessageBoxModule.Singleton.SendMessage(partyLeaderClient, invitedPlayer.GetName() + " has declined your party invitation.");
            }

            MessageBoxModule.Singleton.SendMessage(invitedClient, "You have declined the party invitation from " + invitedBy.GetName());
        }

        private void OnPartyDeclineFail(NetworkConnection invitedClient, string errorMessage)
        {
            MessageBoxModule.Singleton.SendMessage(invitedClient, errorMessage);
        }

        [ServerRpc(RequireOwnership = false)]
        public void LeaveParty(NetworkConnection client = null)
        {
            PartyModule.Singleton.LeaveParty(OnPartyLeaveSuccess, OnPartyLeaveFail, client);
        }

        private void OnPartyLeaveSuccess(NetworkConnection invitedClient, IParty party)
        {
            NotifyPlayerLeave(invitedClient);

            if (party.GetMemberCount() <= 0)
                return;

            PlayerCharacter invitedPlayer = ObjectUtil.FindFirstByType<PlayerController>(invitedClient.Objects).GetPlayerCharacter();

            PlayerCharacter partyLeader = party.GetPartyLeader();
            NetworkConnection leaderClient = partyLeader.GetNetworkConnection();

            ConnectionModule.Singleton.GroupRpc(party.GetConnections(), (Action<NetworkConnection, uint, int, int>)NotifyPartyOnPlayerLeave, invitedPlayer.GetId(), invitedClient.ClientId, leaderClient.ClientId);
        }

        private void OnPartyLeaveFail(NetworkConnection invitedClient, string errorMessage)
        {
            MessageBoxModule.Singleton.SendMessage(invitedClient, errorMessage);
        }

        [TargetRpc]
        private void NotifyPlayerLeave(NetworkConnection leftClient)
        {
            PartyController.Singleton.OnPartyLeave();
        }

        [TargetRpc]
        private void NotifyPartyOnPlayerLeave(NetworkConnection partyClient, uint formerMemberId, int formerClientId, int leaderClientId)
        {
            bool isNewLeader = leaderClientId == partyClient.ClientId;
            OnPlayerLeft?.Invoke(formerMemberId, formerClientId, isNewLeader);
        }

        [ServerRpc(RequireOwnership = false)]
        public void KickPlayer(PlayerCharacter kickedPlayerCharacter, NetworkConnection client = null)
        {
            PartyModule.Singleton.KickPlayer(kickedPlayerCharacter, OnPartyKickSuccess, OnPartyKickFail, client);
        }

        private void OnPartyKickSuccess(NetworkConnection leaderClient, PlayerCharacter kickedPlayer, NetworkConnection kickedClient)
        {
            int kickedPlayerId = GameServerController.Singleton.PlayerList[kickedPlayer.GetId()];

            NotifyPlayerKick(kickedClient);

            IParty party = PlayerController.FindByConnection(leaderClient).GetParty();

            if (party.GetMemberCount() <= 0)
                return;

            ConnectionModule.Singleton.GroupRpc(party.GetConnections(), (Action<NetworkConnection, uint, int>)NotifyPartyOnPlayerKick, kickedPlayer.GetId(), kickedClient.ClientId);
        }

        private void OnPartyKickFail(NetworkConnection invitedClient, string errorMessage)
        {
            MessageBoxModule.Singleton.SendMessage(invitedClient, errorMessage);
        }
        
        [TargetRpc]
        private void NotifyPlayerKick(NetworkConnection kickedClient)
        {
            PartyController.Singleton.OnPartyKick();
        }

        [TargetRpc]
        private void NotifyPartyOnPlayerKick(NetworkConnection partyClient, uint formerMemberId, int formerClientId)
        {
            OnPlayerKick?.Invoke(formerMemberId, formerClientId);
        }
    }
}
