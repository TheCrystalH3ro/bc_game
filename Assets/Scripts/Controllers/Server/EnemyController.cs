using System;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers.Server
{
    public class EnemyController : BaseCharacterController
    {
        [SerializeField] private string enemyName;

        public NetworkObject Prefab;

        public IEnemy Character { get; private set; }

        public static event Action<EnemyController> EnemySpawned;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            Character = new Enemy(enemyName);

            EnemySpawned?.Invoke(this);
        }
    }
}