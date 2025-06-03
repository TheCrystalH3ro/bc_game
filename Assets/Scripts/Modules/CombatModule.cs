using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using FishNet.Connection;
using FishNet.Object;

namespace Assets.Scripts.Modules
{
    public class CombatModule : NetworkBehaviour
    {
        private static CombatModule _instance;

        public static CombatModule Instance
        {
            get
            {
                _instance ??= FindFirstObjectByType<CombatModule>();

                return _instance;
            }
        }

        private List<PlayerController> players;
        private Dictionary<uint, EnemyController> enemies;
        private Dictionary<uint, EnemyController> deadEnemies = new();

        public static event Action<List<PlayerController>> CombatEnded;

        public void StartCombat(List<PlayerController> players, List<EnemyController> enemies)
        {
            this.players = players;
            this.enemies = enemies.ToDictionary(enemy => enemy.Id, enemy => enemy);
        }

        public bool IsValidAttack(PlayerController attacker, uint enemyId)
        {
            if (!players.Contains(attacker))
                return false;

            if (!enemies.ContainsKey(enemyId))
                return false;

            EnemyController enemy = enemies[enemyId];

            return enemies.ContainsValue(enemy);
        }

        public void AttackEnemy(PlayerController attacker, uint enemyId)
        {
            EnemyController enemy = enemies[enemyId];

            HealthModule enemyHealth = enemy.GetComponent<HealthModule>();
            int enemyHp = enemyHealth.TakeHP(10);

            if (enemyHp <= 0)
                EnemyDeath(enemy);
        }

        private void EnemyDeath(EnemyController enemy)
        {
            deadEnemies.Add(enemy.Id, enemy);
            enemies.Remove(enemy.Id);

            if (enemies.Count > 0)
                return;

            EndCombat();
        }

        private void EndCombat()
        {
            CombatEnded?.Invoke(players);
        }
    }
}