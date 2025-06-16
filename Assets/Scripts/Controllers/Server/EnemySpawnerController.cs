using System.Collections;
using Assets.Scripts.Modules;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers.Server
{
    public class EnemySpawnerController : SpawnerModule
    {
        [SerializeField] private Collider2D roamArea;
        [SerializeField] private float respawnTime = 10f;

        public override NetworkObject SpawnObject()
        {
            NetworkObject enemyObject = base.SpawnObject();

            if (enemyObject.TryGetComponent<EnemyController>(out var enemyController))
            {
                enemyController.Prefab = objectToSpawn;
                enemyController.EnemyDespawned += OnEnemyDespawn;
            }

            if (!enemyObject.TryGetComponent<RoamMovementModule>(out var movementModule))
            {
                return enemyObject;
            }

            movementModule.SetRoamArea(roamArea);

            return enemyObject;
        }

        private void OnEnemyDespawn(EnemyController enemy)
        {
            enemy.EnemyDespawned -= OnEnemyDespawn;
            StartCoroutine(RespawnEnemy());
        }

        private IEnumerator RespawnEnemy()
        {
            yield return new WaitForSeconds(respawnTime);

            SpawnObject();
        }
    }
}