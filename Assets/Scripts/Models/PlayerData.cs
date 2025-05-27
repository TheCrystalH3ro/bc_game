using Assets.Scripts.Interfaces;
using Assets.Scripts.Responses;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class PlayerData : IPlayerData
    {
        private PlayerCharacter character;
        private Vector3 position;
        private string scene;

        public PlayerData(PlayerCharacter playerCharacter, Vector3 playerPos, string sceneName)
        {
            character = playerCharacter;
            position = playerPos;
            scene = sceneName;
        }

        public void SetPlayerCharacter(PlayerCharacter playerCharacter)
        {
            this.character = playerCharacter;
        }

        public PlayerCharacter GetPlayerCharacter()
        {
            return this.character;
        }

        public ushort GetLevel()
        {
            return character.GetLevel();
        }

        public float GetExperience()
        {
            return character.GetExp();
        }

        public WorldData GetWorldData()
        {
            PositionData pos = new(position);
            return new(scene, pos);
        }
    }
}
