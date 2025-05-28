using UnityEngine;
using FishNet.Object;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;

namespace Assets.Scripts.Controllers.Server
{
    public class EnemyController : NetworkBehaviour
    {
        [SerializeField] private string enemyName;

        public IEnemy Character { get; private set; }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            Character = new Enemy(enemyName);
        }

    }
}