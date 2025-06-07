using System;
using System.Collections.Generic;
using Assets.Scripts.Models;
using FishNet.Connection;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Interfaces
{
    public interface IParty
    {
        public void AddMember(int clientId, PlayerCharacter member);

        public void RemoveMember(int clientId);

        public void RemoveMember(PlayerCharacter character);

        public bool IsMember(int clientId);

        public bool IsMember(PlayerCharacter character);

        public bool IsLeader(int clientId);
        public bool IsLeader(PlayerCharacter character);

        public void ChangeLeader(int newLeaderId);

        public PlayerCharacter GetPartyLeader();

        public Dictionary<int, PlayerCharacter> GetPlayers();

        public List<PlayerCharacter> GetMembers();

        public PlayerCharacter GetMemberById(uint id);

        public List<NetworkConnection> GetConnections();
        public List<NetworkConnection> GetConnectionsInScene(string scene);
        public List<NetworkConnection> GetConnectionsInScene(int scene);
        public List<NetworkConnection> GetConnectionsInScene(Scene scene);

        public int GetMemberCount();
    }
}