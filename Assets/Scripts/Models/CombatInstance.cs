using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Interfaces;
using FishNet.Connection;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class CombatInstance : ICombatInstance
    {
        private List<PlayerController> players;
        private Dictionary<uint, EnemyController> enemies;
        private Dictionary<uint, EnemyController> deadEnemies = new();

        public CombatInstance(List<PlayerController> players, List<EnemyController> enemies)
        {
            this.players = players;
            this.enemies = enemies.ToDictionary(enemy => enemy.Id, enemy => enemy);
        }

        public EnemyController GetEnemy(uint enemyId)
        {
            if (!enemies.ContainsKey(enemyId))
                return null;

            return enemies[enemyId];
        }

        public int GetEnemyCount()
        {
            return enemies.Count;
        }

        public List<PlayerController> GetPlayers()
        {
            return players;
        }

        public bool IsValidTarget(PlayerController attacker, EnemyController target)
        {
            if (!players.Contains(attacker))
                return false;

            return enemies.ContainsValue(target);
        }

        public void KillEnemy(uint enemyId)
        {
            if (!enemies.ContainsKey(enemyId))
                return;

            EnemyController enemy = enemies[enemyId];

            deadEnemies.Add(enemyId, enemy);
            enemies.Remove(enemyId);
        }
    }
}