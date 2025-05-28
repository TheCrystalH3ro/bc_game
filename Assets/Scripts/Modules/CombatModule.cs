using Assets.Scripts.Controllers;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using FishNet.Connection;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class CombatModule
    {
        private static CombatModule _instance;

        public static CombatModule Singleton
        {
            get
            {
                _instance ??= new();

                return _instance;
            }
        }

        public void StartCombat(PlayerController player, IEnemy enemy)
        {
            PlayerCharacter playerCharacter = player.GetPlayerCharacter();

            Debug.Log(playerCharacter.GetName() + " started combat with " + enemy.GetName());
        }
    }
}