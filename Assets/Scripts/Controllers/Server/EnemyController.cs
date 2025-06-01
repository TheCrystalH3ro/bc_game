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
        [SerializeField] private int enemyLevel;

        public NetworkObject Prefab;

        public IEnemy Character { get; private set; }

        public uint Id { get; private set; }

        public static uint lastId = 0;

        public static event Action<EnemyController> EnemySpawned;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            Character = new Enemy(enemyName, enemyLevel);

            Id = lastId++;

            EnemySpawned?.Invoke(this);
        }

        public override CharacterData ToCharacterData()
        {
            return base.ToCharacterData(Id, Character.GetName(), Character.GetLevel());
        }
    }
}