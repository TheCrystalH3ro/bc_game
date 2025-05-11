using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using FishNet;
using FishNet.Connection;
using FishNet.Object;

namespace Assets.Scripts.Modules
{
    public class PartyModule : NetworkBehaviour
    {
        private static PartyModule  _instance;

        public static PartyModule Singleton
        { 
            get
            {
                _instance ??=  new();

                return _instance;
            }
        }

        private Dictionary<int, int> partyInvites = new();

        public void UpdatePartyUI(List<PlayerCharacter> playerCharacters, uint leaderId)
        {
        
        }

        public void InvitePlayer(uint invitedPlayerId, Action<NetworkConnection, PlayerCharacter> onInviteSuccess = null, Action<NetworkConnection, PlayerCharacter, string> onInviteFail = null, NetworkConnection client = null)
        {
            int invitedClientId = GameServerController.Singleton.PlayerList[invitedPlayerId];

            //Invited player already has a pending invitation
            if(partyInvites.ContainsKey(invitedClientId))
            {
                onInviteFail?.Invoke(client, null, "");
                return;
            }

            //Player doesn't exist (possibly disconnected)
            if(client == null) {
                onInviteFail?.Invoke(client, null, "");
                return;
            }

            NetworkConnection invitedClient = InstanceFinder.ServerManager.Clients[invitedClientId];

            //Invited player doesn't exist (possibly disconnected)
            if(invitedClient == null)
            {
                onInviteFail?.Invoke(client, null, "");
                return;
            }

            PlayerController invitedPlayer = invitedClient.FirstObject.GetComponent<PlayerController>();

            PlayerController invitedFrom = client.FirstObject.GetComponent<PlayerController>();

            uint invitedFromId = invitedFrom.GetPlayerCharacter().GetId();

            Party party = invitedFrom.GetParty();

            if(party != null)
            {
                //Only party leader can invite new members
                if(!invitedFrom.GetParty().IsLeader(invitedFromId))
                {
                    onInviteFail?.Invoke(client, invitedPlayer.GetPlayerCharacter(), "");
                    return;
                }

                //Party is already full
                if(party.GetMemberCount() >= 4)
                {
                    onInviteFail?.Invoke(client, invitedPlayer.GetPlayerCharacter(), "");
                    return;
                }
            }

            //Invited player is already in party
            if(invitedPlayer.GetParty() != null)
            {
                onInviteFail?.Invoke(client, invitedPlayer.GetPlayerCharacter(), "");
                return;
            }

            partyInvites.Add(invitedClientId, client.ClientId);
            onInviteSuccess?.Invoke(client, invitedPlayer.GetPlayerCharacter());
        }

        public void AcceptInvite(Action<NetworkConnection> onAcceptSuccess = null, Action<NetworkConnection, string> onAcceptFail = null, NetworkConnection invitedClient = null)
        {
            int clientId = invitedClient.ClientId;

            //Player doesn't exist (possibly disconnected)
            if(invitedClient == null) return;

            //Player has no invitation
            if(!partyInvites.ContainsKey(clientId))
            {
                onAcceptFail?.Invoke(invitedClient, "");
                return;
            }

            int invitedById = partyInvites[clientId];

            NetworkConnection partyLeaderClient = InstanceFinder.ServerManager.Clients[invitedById];

            //Party leader doesn't exist (possibly disconnected)
            if(partyLeaderClient == null)
            {
                onAcceptFail?.Invoke(invitedClient, "");
                return;
            }

            PlayerController invitedPlayer = invitedClient.FirstObject.GetComponent<PlayerController>();
        
            //Player is already in a party
            if(invitedPlayer.GetParty() != null)
            {
                onAcceptFail?.Invoke(invitedClient, "");
                return;
            }

            PlayerController partyLeader = partyLeaderClient.FirstObject.GetComponent<PlayerController>();
            PlayerCharacter partyLeaderCharacter = partyLeader.GetPlayerCharacter();

            Party party;

            if (partyLeader.GetParty() == null)
            {
                party = new(partyLeaderCharacter, invitedById);
                partyLeader.SetParty(party);
            } else 
            {
                party = partyLeader.GetParty();

                //Player is no longer party leader
                if(!party.IsLeader(partyLeaderCharacter.GetId()))
                {
                    onAcceptFail?.Invoke(invitedClient, "");
                    return;
                }

                //Party is already full
                if(party.GetMemberCount() >= 4)
                {
                    onAcceptFail?.Invoke(invitedClient, "");
                    return;
                }
            }

            PlayerCharacter invitedPlayerCharacter = invitedPlayer.GetPlayerCharacter();
            party.AddMember(invitedPlayerCharacter, clientId);

            invitedPlayer.SetParty(party);
            partyInvites.Remove(clientId);

            onAcceptSuccess?.Invoke(invitedClient);
        }

        public void DeclineInvite(Action<NetworkConnection, PlayerCharacter, Party> onDeclineSuccess = null, Action<NetworkConnection, string> onDeclineFail = null, NetworkConnection invitedClient  = null)
        {
            int clientId = invitedClient.ClientId;

            //Player has no invitation
            if(!partyInvites.ContainsKey(clientId))
            {
                onDeclineFail?.Invoke(invitedClient, "");
                return;
            }

            int invitedById = partyInvites[clientId];

            NetworkConnection partyLeaderClient = InstanceFinder.ServerManager.Clients[invitedById];

            //Party leader doesn't exist (possibly disconnected)
            if(partyLeaderClient == null)
            {
                onDeclineFail?.Invoke(invitedClient, "");
                return;
            }

            PlayerController partyLeader = partyLeaderClient.FirstObject.GetComponent<PlayerController>();

            partyInvites.Remove(clientId);
            onDeclineSuccess?.Invoke(invitedClient, partyLeader.GetPlayerCharacter(), partyLeader.GetParty());
        }

        public void LeaveParty(Action<NetworkConnection, Party> onLeaveSuccess = null, Action<NetworkConnection, string> onLeaveFail = null, NetworkConnection client  = null)
        {
            //Player doesn't exist (possibly disconnected)
            if(client == null) return;

            PlayerController playerController = client.FirstObject.GetComponent<PlayerController>();

            Party party = playerController.GetParty();

            //Player is not in a party
            if(party == null)
            {
                onLeaveFail?.Invoke(client, "");
                return;
            }

            PlayerCharacter playerCharacter = playerController.GetPlayerCharacter();

            party.RemoveMember(playerCharacter.GetId());

            //Set new leader
            if(party.IsLeader(playerCharacter.GetId()) && party.GetMemberCount() > 0)
            {
                Dictionary<uint, PlayerCharacter> members = party.GetMembers();
                uint newLeaderId = members.Keys.First();
                party.ChangeLeader(newLeaderId);
            }

            playerController.SetParty(null);

            onLeaveSuccess?.Invoke(client, party);
        }

        public void KickPlayer(PlayerCharacter kickedPlayerCharacter, Action<NetworkConnection, PlayerCharacter> onKickSuccess = null, Action<NetworkConnection, string> onKickFail = null, NetworkConnection partyLeaderClient  = null)
        {
            //Party leader doesn't exist (possibly disconnected)
            if(partyLeaderClient == null)  return;

            PlayerController partyLeader = partyLeaderClient.FirstObject.GetComponent<PlayerController>();
            Party party = partyLeader.GetParty();

            //Player is not in a party
            if(party == null)
            {
                onKickFail?.Invoke(partyLeaderClient, "");
                return;
            }

            PlayerCharacter partyLeaderCharacter = partyLeader.GetPlayerCharacter();

            //Player is not a party leader
            if(!party.IsLeader(partyLeaderCharacter.GetId()))
            {
                onKickFail?.Invoke(partyLeaderClient, "");
                return;
            }

            //Player is not member of the party
            if(!party.IsMember(kickedPlayerCharacter.GetId()))
            {
                onKickFail?.Invoke(partyLeaderClient, "");
                return;
            }

            NetworkConnection kickedClient = party.GetPlayerClient(kickedPlayerCharacter.GetId());

            party.RemoveMember(kickedPlayerCharacter.GetId());

            PlayerController kickedPlayer = null;

            if(kickedClient != null)
            {
                kickedPlayer = kickedClient.FirstObject.GetComponent<PlayerController>();
                kickedPlayer.SetParty(null);
            }

            onKickSuccess?.Invoke(partyLeaderClient, kickedPlayer.GetPlayerCharacter());
            //TODO: Update local server stored data
        }
    }
}