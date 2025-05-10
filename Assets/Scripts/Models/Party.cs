using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using FishNet;
using FishNet.Connection;

namespace Assets.Scripts.Models
{
    public class Party
    {
        private uint leaderId;
        private Dictionary<uint, PlayerCharacter> members;
        private Dictionary<uint, int> playerSessions;

        public Party(PlayerCharacter leader, int clientId) {
            leaderId = leader.GetId();
            members = new Dictionary<uint, PlayerCharacter>
            {
                { leaderId, leader }
            };
            playerSessions = new Dictionary<uint, int>
            {
                { leaderId, clientId }
            };
        }

        private void UpdatePlayers() {
            GameServerController.Singleton.UpdatePartyStatus(this);
        }

        public void AddMember(PlayerCharacter member, int clientId) {
            members.Add(member.GetId(), member);
            playerSessions.Add(member.GetId(), clientId);
            UpdatePlayers();
        }

        public void RemoveMember(uint characterId) {
            members.Remove(characterId);
            playerSessions.Remove(characterId);
            UpdatePlayers();
        }

        public bool IsMember(uint characterId) {
            return members.ContainsKey(characterId);
        }

        public bool IsLeader(uint characterId) {
            return leaderId == characterId;
        }

        public void ChangeLeader(uint newLeaderId) {
            if(!IsMember(newLeaderId)) {
                return;
            }

            leaderId = newLeaderId;
        }

        public PlayerCharacter GetPartyLeader() {
            return members[leaderId];
        }

        public Dictionary<uint, PlayerCharacter> GetMembers() {
            return members;
        }

        public int GetMemberCount() {
            return members.Count;
        }

        public NetworkConnection GetPlayerClient(uint characterId) {
            if(!IsMember(characterId)) {
                return null;
            }

            int clientId = playerSessions[characterId];

            if(clientId == 0) {
                return null;
            }

            return InstanceFinder.ServerManager.Clients[clientId];
        }

        public ulong[] GetClientIds() {
            List<ulong> clientIds = new();

            foreach(ulong clientId in playerSessions.Values) {
                if(clientId != 0) {
                    clientIds.Add(clientId);
                }
            }

            return clientIds.ToArray();
        }

        public void PlayerConnected(uint characterId, int clientId) {
            if(!IsMember(characterId)) {
                return;
            }

            playerSessions[characterId] = clientId;
        }

        public void PlayerDisconnected(uint characterId) {
            if(!IsMember(characterId)) {
                return;
            }

            playerSessions[characterId] = 0;
        }
    }
}