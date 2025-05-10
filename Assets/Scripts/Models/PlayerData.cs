using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class PlayerData : MonoBehaviour
    {
        private PlayerCharacter character;

        public void SetPlayerCharacter(PlayerCharacter playerCharacter) {
            this.character = playerCharacter;
        }

        public PlayerCharacter GetPlayerCharacter() {
            return this.character;
        }
    }
}
