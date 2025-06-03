using System;
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

        public static event Action<ICombatInstance> CombatEnded;

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

        public bool IsValidAttack(PlayerController attacker, uint enemyId)
        {
            ICombatInstance instance = GetInstance(attacker);
            EnemyController enemy = instance.GetEnemy(enemyId);

            return instance.IsValidTarget(attacker, enemy);
        }

        public void AttackEnemy(PlayerController attacker, uint enemyId)
        {
            ICombatInstance instance = GetInstance(attacker);
            EnemyController enemy = instance.GetEnemy(enemyId);

            HealthModule enemyHealth = enemy.GetComponent<HealthModule>();
            int enemyHp = enemyHealth.TakeHP(10);

            if (enemyHp <= 0)
                EnemyDeath(instance, enemy);
        }

        private void EnemyDeath(ICombatInstance instance, EnemyController enemy)
        {
            instance.KillEnemy(enemy.Id);

            if (instance.GetEnemyCount() > 0)
                return;

            EndCombat(instance);
        }

        private void EndCombat(ICombatInstance instance)
        {
            CombatEnded?.Invoke(instance);

            foreach (PlayerController player in instance.GetPlayers())
            {
                NetworkConnection connection = player.Owner;

                if (!instances.ContainsKey(connection))
                    continue;

                instances.Remove(connection);
            }
        }
    }
}