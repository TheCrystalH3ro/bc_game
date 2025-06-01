using Assets.Scripts.Modules;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers.Server
{
    public class EnemySpawnerController : SpawnerModule
    {
        [SerializeField] private Collider2D roamArea;

        public override NetworkObject SpawnObject()
        {
            NetworkObject enemyObject = base.SpawnObject();

            if (enemyObject.TryGetComponent<EnemyController>(out var enemyController))
            {
                enemyController.Prefab = objectToSpawn;
            }

            if (!enemyObject.TryGetComponent<RoamMovementModule>(out var movementModule))
            {
                return enemyObject;
            }

            movementModule.SetRoamArea(roamArea);

            return enemyObject;
        }
    }
}