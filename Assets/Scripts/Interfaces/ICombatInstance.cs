using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;

namespace Assets.Scripts.Interfaces
{
    public interface ICombatInstance
    {
        public bool IsValidTarget(PlayerController attacker, EnemyController target);
        public EnemyController GetEnemy(uint enemyId);
    }
}