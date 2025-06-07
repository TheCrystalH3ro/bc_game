using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Interfaces;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class Party : IParty
    {
        private int leaderId;

        Dictionary<int, PlayerCharacter> Members { get; set; }

        public Party(PlayerCharacter leader, int clientId)
        {
            leaderId = clientId;
            Members = new Dictionary<int, PlayerCharacter>
            {
                { leaderId, leader }
            };
        }

        public Party(int leaderId, Dictionary<int, PlayerCharacter> members)
        {
            this.leaderId = leaderId;
            Members = members;
        }

        public void AddMember(int clientId, PlayerCharacter member)
        {
            Members.Add(clientId, member);
        }

        public void RemoveMember(int clientId)
        {
            Members.Remove(clientId);
        }

        public void RemoveMember(PlayerCharacter character)
        {
            int connectionId = character.GetConnectionId();

            if (connectionId < 0 || !Members.ContainsKey(connectionId)) return;

            RemoveMember(connectionId);
        }

        public bool IsMember(int clientId)
        {
            return Members.ContainsKey(clientId);
        }

        public bool IsMember(PlayerCharacter character)
        {
            int connectionId = character.GetConnectionId();

            if (connectionId < 0) return false;

            return IsMember(connectionId);
        }

        public bool IsLeader(int clientId)
        {
            return leaderId == clientId;
        }

        public bool IsLeader(PlayerCharacter character)
        {
            int connectionId = character.GetConnectionId();

            return IsLeader(connectionId);
        }

        public void ChangeLeader(int newLeaderId)
        {
            if (!IsMember(newLeaderId))
            {
                return;
            }

            leaderId = newLeaderId;
        }

        public PlayerCharacter GetPartyLeader()
        {
            return Members[leaderId];
        }

        public Dictionary<int, PlayerCharacter> GetPlayers()
        {
            return Members;
        }

        public List<PlayerCharacter> GetMembers()
        {
            return Members.Values.ToList();
        }

        public PlayerCharacter GetMemberById(uint id)
        {
            foreach (PlayerCharacter member in Members.Values)
                if (member.GetId() == id) return member;

            return null;
        }

        public List<NetworkConnection> GetConnections()
        {
            return Members.Keys.Select(clientId => InstanceFinder.ServerManager.Clients[clientId]).ToList();
        }

        public List<NetworkConnection> GetConnectionsInScene(string scene)
        {
            return GetConnectionsInScene(SceneManager.GetScene(scene));
        }

        public List<NetworkConnection> GetConnectionsInScene(int scene)
        {
            return GetConnectionsInScene(SceneManager.GetScene(scene));
        }

        public List<NetworkConnection> GetConnectionsInScene(UnityEngine.SceneManagement.Scene scene)
        {
            return Members.Keys.Select(clientId => InstanceFinder.ServerManager.Clients[clientId]).Where(player => player.FirstObject.gameObject.scene == scene).ToList();
        }

        public int GetMemberCount()
        {
            return Members.Count;
        }
    }
}