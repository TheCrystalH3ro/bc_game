using System.Collections.Generic;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using FishNet.Connection;

namespace Assets.Scripts.Interfaces
{
    public interface ICombatInstance
    {
        public bool IsValidTarget(PlayerController attacker, EnemyController target);
        public EnemyController GetEnemy(uint enemyId);
        public void KillEnemy(uint enemyId);
        public int GetEnemyCount();
        public List<PlayerController> GetPlayers();
    }
}