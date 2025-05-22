using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Util;
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
                _instance ??=  FindFirstObjectByType<PartyModule>();

                return _instance;
            }
        }

        private Dictionary<int, int> partyInvites = new();

        public void InvitePlayer(PlayerCharacter invitedCharacter, Action<NetworkConnection, NetworkConnection, PlayerCharacter> onInviteSuccess = null, Action<NetworkConnection, PlayerCharacter, string> onInviteFail = null, NetworkConnection client = null)
        {
            //Player doesn't exist (possibly disconnected)
            if(client == null) return;

            int invitedClientId = invitedCharacter.GetConnectionId();

            if(partyInvites.ContainsKey(invitedClientId))
            {
                onInviteFail?.Invoke(client, null, "Invited player already has a pending invitation.");
                return;
            }

            NetworkConnection invitedClient = invitedCharacter.GetNetworkConnection();

            //Invited player doesn't exist (possibly disconnected)
            if(invitedClient == null)
            {
                onInviteFail?.Invoke(client, null, "Couldn't find invited player.");
                return;
            }

            PlayerController invitedPlayer = invitedCharacter.GetPlayerController();

            PlayerController invitedFrom = ObjectUtil.FindFirstByType<PlayerController>(client.Objects);

            PlayerCharacter invitedFromCharacter = invitedFrom.GetPlayerCharacter();

            uint invitedFromId = invitedFromCharacter.GetId();

            IParty party = invitedFrom.GetParty();

            if(party != null)
            {
                if(! invitedFrom.GetParty().IsLeader(invitedFromCharacter) )
                {
                    onInviteFail?.Invoke(client, invitedPlayer.GetPlayerCharacter(), "Only party leader can invite new members.");
                    return;
                }

                if(party.GetMemberCount() >= PartyServerController.MAX_PARTY_MEMBERS)
                {
                    onInviteFail?.Invoke(client, invitedPlayer.GetPlayerCharacter(), "Party is already full.");
                    return;
                }
            }

            if(invitedPlayer.GetParty() != null)
            {
                onInviteFail?.Invoke(client, invitedPlayer.GetPlayerCharacter(), "Invited player is already in a party.");
                return;
            }

            partyInvites.Add(invitedClientId, client.ClientId);
            onInviteSuccess?.Invoke(invitedClient, client, invitedFromCharacter);
        }

        public void AcceptInvite(Action<NetworkConnection, NetworkConnection, PlayerCharacter, IParty> onAcceptSuccess = null, Action<NetworkConnection, string> onAcceptFail = null, NetworkConnection invitedClient = null)
        {
            int clientId = invitedClient.ClientId;

            //Player doesn't exist (possibly disconnected)
            if(invitedClient == null) return;

            if(!partyInvites.ContainsKey(clientId))
            {
                onAcceptFail?.Invoke(invitedClient, "You have no invitations.");
                return;
            }

            int invitedById = partyInvites[clientId];

            NetworkConnection partyLeaderClient = InstanceFinder.ServerManager.Clients[invitedById];

            //Party leader doesn't exist (possibly disconnected)
            if(partyLeaderClient == null)
            {
                onAcceptFail?.Invoke(invitedClient, "Couldn't find the party.");
                return;
            }

            PlayerController invitedPlayer = ObjectUtil.FindFirstByType<PlayerController>(invitedClient.Objects);
        
            //Player is already in a party
            if(invitedPlayer.GetParty() != null)
            {
                onAcceptFail?.Invoke(invitedClient, "You are already in a party.");
                return;
            }

            PlayerController partyLeader = ObjectUtil.FindFirstByType<PlayerController>(partyLeaderClient.Objects);
            PlayerCharacter partyLeaderCharacter = partyLeader.GetPlayerCharacter();

            IParty party;

            if (partyLeader.GetParty() == null)
            {
                party = new Party(partyLeaderCharacter, invitedById);
                partyLeader.SetParty(party);
            } else 
            {
                party = partyLeader.GetParty();

                //Player is no longer party leader
                if(! party.IsLeader(partyLeaderCharacter))
                {
                    onAcceptFail?.Invoke(invitedClient, "Player is no longer party leader.");
                    return;
                }

                //Party is already full
                if(party.GetMemberCount() >= 4)
                {
                    onAcceptFail?.Invoke(invitedClient, "Party is already full.");
                    return;
                }
            }

            PlayerCharacter invitedPlayerCharacter = invitedPlayer.GetPlayerCharacter();
            party.AddMember(clientId, invitedPlayerCharacter);

            invitedPlayer.SetParty(party);
            partyInvites.Remove(clientId);

            onAcceptSuccess?.Invoke(invitedClient, partyLeaderClient, invitedPlayerCharacter, party);
        }

        public void DeclineInvite(Action<NetworkConnection, NetworkConnection> onDeclineSuccess = null, Action<NetworkConnection, string> onDeclineFail = null, NetworkConnection invitedClient  = null)
        {
            int clientId = invitedClient.ClientId;

            //Player has no invitation
            if(!partyInvites.ContainsKey(clientId))
            {
                onDeclineFail?.Invoke(invitedClient, "You have no invitation.");
                return;
            }

            int invitedById = partyInvites[clientId];

            partyInvites.Remove(clientId);

            NetworkConnection partyLeaderClient = InstanceFinder.ServerManager.Clients[invitedById];

            //Party leader doesn't exist (possibly disconnected)
            if(partyLeaderClient == null)
            {
                onDeclineFail?.Invoke(invitedClient, "Couldn't find the party");
                return;
            }

            onDeclineSuccess?.Invoke(invitedClient, partyLeaderClient);
        }

        public void LeaveParty(Action<NetworkConnection, IParty> onLeaveSuccess = null, Action<NetworkConnection, string> onLeaveFail = null, NetworkConnection client  = null)
        {
            //Player doesn't exist (possibly disconnected)
            if(client == null) return;

            PlayerController playerController = ObjectUtil.FindFirstByType<PlayerController>(client.Objects);

            IParty party = playerController.GetParty();

            //Player is not in a party
            if(party == null)
            {
                onLeaveFail?.Invoke(client, "You are not in a party.");
                return;
            }

            PlayerCharacter playerCharacter = playerController.GetPlayerCharacter();

            party.RemoveMember(playerCharacter);

            //Set new leader
            if (party.IsLeader(playerCharacter) && party.GetMemberCount() > 0)
            {
                Dictionary<int, PlayerCharacter> members = party.GetPlayers();
                int newLeaderId = members.Keys.First();
                party.ChangeLeader(newLeaderId);
            }

            if (party.GetMemberCount() <= 1)
            {
                PlayerCharacter partyLeader = party.GetPartyLeader();
                PlayerController partyLeaderController = partyLeader.GetPlayerController();
                partyLeaderController.SetParty(null);
            }

            playerController.SetParty(null);

            onLeaveSuccess?.Invoke(client, party);
        }

        public void KickPlayer(PlayerCharacter kickedPlayerCharacter, Action<NetworkConnection, PlayerCharacter, NetworkConnection> onKickSuccess = null, Action<NetworkConnection, string> onKickFail = null, NetworkConnection partyLeaderClient  = null)
        {
            //Party leader doesn't exist (possibly disconnected)
            if(partyLeaderClient == null)  return;

            PlayerController partyLeader = ObjectUtil.FindFirstByType<PlayerController>(partyLeaderClient.Objects);
            IParty party = partyLeader.GetParty();

            //Player is not in a party
            if(party == null)
            {
                onKickFail?.Invoke(partyLeaderClient, "Player is no longer in a party.");
                return;
            }

            PlayerCharacter partyLeaderCharacter = partyLeader.GetPlayerCharacter();

            //Player is not a party leader
            if(!party.IsLeader(partyLeaderCharacter))
            {
                onKickFail?.Invoke(partyLeaderClient, "You are not the party leader.");
                return;
            }

            //Player is not member of the party
            if(!party.IsMember(kickedPlayerCharacter))
            {
                onKickFail?.Invoke(partyLeaderClient, "Player is not a member of your party.");
                return;
            }

            NetworkConnection kickedClient = kickedPlayerCharacter.GetNetworkConnection();

            party.RemoveMember(kickedPlayerCharacter);

            PlayerController kickedPlayer = null;
            PlayerCharacter kickedCharacter = null;

            if (kickedClient != null)
            {
                kickedPlayer = PlayerController.FindByConnection(kickedClient);
                kickedPlayer.SetParty(null);
                kickedCharacter = kickedPlayer.GetPlayerCharacter();
            }

            if (party.GetMemberCount() <= 1)
            {
                partyLeader.SetParty(null);
            }

            onKickSuccess?.Invoke(partyLeaderClient, kickedCharacter, kickedClient);
        }
    }
}