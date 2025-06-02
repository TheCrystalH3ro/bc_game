using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class CombatInstance : ICombatInstance
    {
        private List<PlayerController> players;
        private Dictionary<uint, EnemyController> enemies;

        public CombatInstance(List<PlayerController> players, List<EnemyController> enemies)
        {
            this.players = players;
            this.enemies = enemies.ToDictionary(enemy => enemy.Id, enemy => enemy);
            foreach (uint enemyId in this.enemies.Keys)
            {
                Debug.Log("Id of enemy is " + enemyId);
            }
        }

        public EnemyController GetEnemy(uint enemyId)
        {
            if (!enemies.ContainsKey(enemyId))
                return null;

            return enemies[enemyId];
        }

        public bool IsValidTarget(PlayerController attacker, EnemyController target)
        {
            if (!players.Contains(attacker))
                return false;

            return enemies.ContainsValue(target);
        }
    }
}