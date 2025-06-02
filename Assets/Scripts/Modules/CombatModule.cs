using System.Collections.Generic;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using FishNet.Connection;
using FishNet.Object;

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

        private Dictionary<NetworkConnection, ICombatInstance> instances = new();

        private ICombatInstance GetInstance(PlayerController player)
        {
            return instances[player.Owner];
        }

        public void StartCombat(List<PlayerController> players, List<EnemyController> enemies)
        {
            ICombatInstance instance = new CombatInstance(players, enemies);

            foreach (PlayerController player in players)
            {
                NetworkConnection connection = player.Owner;

                instances.Add(connection, instance);
            }
        }

        public void AttackEnemy(PlayerController attacker, uint enemyId)
        {
            ICombatInstance instance = GetInstance(attacker);
            EnemyController enemy = instance.GetEnemy(enemyId);

            if (!instance.IsValidTarget(attacker, enemy))
                return;

            HealthModule enemyHealth = enemy.GetComponent<HealthModule>();
            enemyHealth.TakeHP(10);
        }
    }
}