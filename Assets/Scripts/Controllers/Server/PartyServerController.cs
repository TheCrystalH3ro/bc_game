using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Controllers.Server
{
    public class PartyServerController : MonoBehaviour
    {
        // private Dictionary<ulong, ulong> partyInvites;

        // public void OnEnable() {
        //     partyInvites = new();
        // }

        // public void InvitePlayer(ulong clientId, ulong invitedClientId) {

        //     //Invited player already has a pending invitation
        //     if(partyInvites.ContainsKey(invitedClientId)) {
        //         return;
        //     }

        //     NetworkClient partyLeaderClient = NetworkManager.Singleton.ConnectedClients[clientId];

        //     //Player doesn't exist (possibly disconnected)
        //     if(partyLeaderClient == null) {
        //         return;
        //     }

        //     NetworkClient invitedClient = NetworkManager.Singleton.ConnectedClients[invitedClientId];

        //     //Invited player doesn't exist (possibly disconnected)
        //     if(invitedClient == null) {
        //         return;
        //     }

        //     PlayerController partyLeader = partyLeaderClient.PlayerObject.GetComponent<PlayerController>();

        //     uint leaderId = partyLeader.GetPlayerCharacter().GetId();

        //     Party party = partyLeader.GetParty();

        //     if(party != null) {

        //         //Only party leader can invite new members
        //         if(!partyLeader.GetParty().IsLeader(leaderId)) {
        //             return;
        //         }

        //         //Party is already full
        //         if(party.GetMemberCount() >= 4) {
        //             return;
        //         }
        //     }

        //     PlayerController invitedPlayer = invitedClient.PlayerObject.GetComponent<PlayerController>();

        //     //Invited player is already in party
        //     if(invitedPlayer.GetParty() != null) {
        //         return;
        //     }

        //     partyInvites.Add(invitedClientId, clientId);
        //     NotifyPlayersInvite(invitedClientId, clientId);
        // }

        // private void NotifyPlayersInvite(ulong invitedById, ulong invitedId) {

        // }

        // public void AcceptInvite(ulong clientId) {

        //     //Player has no invitation
        //     if(!partyInvites.ContainsKey(clientId)) {
        //         return;
        //     }

        //     ulong invitedById = partyInvites[clientId];

        //     NetworkClient invitedClient = NetworkManager.Singleton.ConnectedClients[clientId];

        //     //Player doesn't exist (possibly disconnected)
        //     if(invitedClient == null) {
        //         return;
        //     }

        //     NetworkClient partyLeaderClient = NetworkManager.Singleton.ConnectedClients[invitedById];

        //     //Party leader doesn't exist (possibly disconnected)
        //     if(partyLeaderClient == null) {
        //         return;
        //     }

        //     PlayerController invitedPlayer = invitedClient.PlayerObject.GetComponent<PlayerController>();
        
        //     //Player is already in a party
        //     if(invitedPlayer.GetParty() != null) {
        //         return;
        //     }

        //     PlayerController partyLeader = partyLeaderClient.PlayerObject.GetComponent<PlayerController>();
        //     PlayerCharacter partyLeaderCharacter = partyLeader.GetPlayerCharacter();

        //     Party party;
        //     if (partyLeader.GetParty() == null) {
        //         party = new(partyLeaderCharacter, invitedById);
        //         partyLeader.SetParty(party);
        //     } else {
        //         party = partyLeader.GetParty();

        //         //Player is no longer party leader
        //         if(!party.IsLeader(partyLeaderCharacter.GetId())) {
        //             return;
        //         }

        //         //Party is already full
        //         if(party.GetMemberCount() >= 4) {
        //             return;
        //         }
        //     }

        //     PlayerCharacter invitedPlayerCharacter = invitedPlayer.GetPlayerCharacter();
        //     party.AddMember(invitedPlayerCharacter, clientId);

        //     invitedPlayer.SetParty(party);
        //     partyInvites.Remove(clientId);
        // }

        // public void DeclineInvite(ulong clientId) {

        //     //Player has no invitation
        //     if(!partyInvites.ContainsKey(clientId)) {
        //         return;
        //     }

        //     ulong invitedById = partyInvites[clientId];

        //     NetworkClient partyLeaderClient = NetworkManager.Singleton.ConnectedClients[invitedById];

        //     //Party leader doesn't exist (possibly disconnected)
        //     if(partyLeaderClient == null) {
        //         return;
        //     }

        //     PlayerController partyLeader = partyLeaderClient.PlayerObject.GetComponent<PlayerController>();
        //     uint leaderId = partyLeader.GetPlayerCharacter().GetId();

        //     //Notify leader
        //     if(partyLeader.GetParty() == null || partyLeader.GetParty().IsLeader(leaderId)) {

        //     }

        //     partyInvites.Remove(clientId);
        // }

        // public void LeaveParty(ulong clientId) {

        //     NetworkClient client = NetworkManager.Singleton.ConnectedClients[clientId];

        //     //Player doesn't exist (possibly disconnected)
        //     if(client == null) {
        //         return;
        //     }

        //     PlayerController playerController = client.PlayerObject.GetComponent<PlayerController>();

        //     Party party = playerController.GetParty();

        //     //Player is not in a party
        //     if(party == null) {
        //         return;
        //     }

        //     PlayerCharacter playerCharacter = playerController.GetPlayerCharacter();

        //     party.RemoveMember(playerCharacter.GetId());

        //     //Set new leader
        //     if(party.IsLeader(playerCharacter.GetId()) && party.GetMemberCount() > 0) {
        //         Dictionary<uint, PlayerCharacter> members = party.GetMembers();
        //         uint newLeaderId = members.Keys.First();
        //         party.ChangeLeader(newLeaderId);
        //     }

        //     playerController.SetParty(null);
        // }

        // public void KickPlayer(uint leaderClientId, PlayerCharacter kickedPlayerCharacter) {

        //     NetworkClient partyLeaderClient = NetworkManager.Singleton.ConnectedClients[leaderClientId];

        //     //Party leader doesn't exist (possibly disconnected)
        //     if(partyLeaderClient == null) {
        //         return;
        //     }

        //     PlayerController partyLeader = partyLeaderClient.PlayerObject.GetComponent<PlayerController>();
        //     Party party = partyLeader.GetParty();

        //     //Player is not in a party
        //     if(party == null) {
        //         return;
        //     }

        //     PlayerCharacter partyLeaderCharacter = partyLeader.GetPlayerCharacter();

        //     //Player is not a party leader
        //     if(!party.IsLeader(partyLeaderCharacter.GetId())) {
        //         return;
        //     }

        //     //Player is not member of the party
        //     if(!party.IsMember(kickedPlayerCharacter.GetId())) {
        //         return;
        //     }

        //     NetworkClient kickedClient = party.GetPlayerClient(kickedPlayerCharacter.GetId());

        //     party.RemoveMember(kickedPlayerCharacter.GetId());

        //     if(kickedClient != null) {
        //         PlayerController kickedPlayer = kickedClient.PlayerObject.GetComponent<PlayerController>();
        //         kickedPlayer.SetParty(null);
        //     }

        //     //TODO: Update local server stored data
        // }

    }
}
