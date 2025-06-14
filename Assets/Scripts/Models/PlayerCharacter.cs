using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Enums;
using Assets.Scripts.Responses;
using FishNet;
using FishNet.Connection;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class PlayerCharacter
    {
        private uint id;
        private string name;
        private ushort level;
        private int health = 0;
        private float experience;
        private PlayerClass playerClass;
        private string currentScene;

        public PlayerCharacter(uint id, string name, ushort level, PlayerClass playerClass)
        {
            this.id = id;
            this.name = name;
            this.level = level;
            this.experience = 0;
            this.playerClass = playerClass;
        }

        public PlayerCharacter(CharacterResponse character)
        {
            this.id = character.id;
            this.name = character.name;
            this.level = character.level;
            this.playerClass = character.characterTypeId;
        }

        public PlayerCharacter()
        {
            this.id = 0;
            this.name = "";
            this.level = 0;
            this.playerClass = PlayerClass.Undefined;
        }

        public bool Equals(PlayerCharacter character)
        {
            return this.id == character.GetId();
        }

        public uint GetId()
        {
            return this.id;
        }

        public string GetName()
        {
            return this.name;
        }

        public ushort GetLevel()
        {
            return this.level;
        }

        public PlayerClass GetPlayerClass()
        {
            return this.playerClass;
        }

        public int GetConnectionId()
        {
            if (!GameServerController.Singleton.PlayerList.ContainsKey(id)) return -1;

            return GameServerController.Singleton.PlayerList[id];
        }

        public NetworkConnection GetNetworkConnection()
        {
            int clientId = GetConnectionId();

            if (clientId < 0 || !InstanceFinder.ServerManager.Clients.ContainsKey(clientId)) return null;

            return InstanceFinder.ServerManager.Clients[clientId];
        }

        public PlayerController GetPlayerController()
        {
            NetworkConnection client = GetNetworkConnection();

            if (client == null) return null;

            return PlayerController.FindByConnection(client);
        }

        public Sprite GetSprite()
        {
            return ClassController.Singleton.GetCharacterSprite(playerClass);
        }

        public int GetHealth()
        {
            return health;
        }

        public void SetHealth(int health)
        {
            this.health = health;
        }

        public float GetExp()
        {
            return this.experience;
        }

        public void LoadData(PlayerDataResponse playerDataResponse)
        {
            this.level = playerDataResponse.level;
            this.experience = playerDataResponse.experience;
        }

        public string GetCurrentScene()
        {
            return currentScene;
        }

        public void SetCurrentScene(string scene)
        {
            currentScene = scene;
        }
    }
}
