using Assets.Scripts.Modules;
using Assets.Scripts.Triggers;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers.Server
{
    public class CombatServerController : NetworkBehaviour
    {
        private static CombatServerController _instance;

        public static CombatServerController Singleton
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CombatServerController>();
                }

                return _instance;
            }
        }

        public static readonly string COMBAT_SCENE_NAME = "Combat";

        public void MoveToCombat(PlayerController player, EnemyController enemy)
        {
            player.EnterCombatRpc();

            SceneModule.Singleton.LoadInstance(player.Owner, COMBAT_SCENE_NAME, instanceLoadEvent: (sceneHandle) =>
            {
                Initialize(player, enemy, sceneHandle);
            });
        }

        public void Initialize(PlayerController player, EnemyController enemy, int sceneHandle)
        {
            GameObject[] enemySlots = GameObject.FindGameObjectsWithTag("EnemySlot");

            SetEnemy(enemy, sceneHandle, enemySlots[0].transform.position);
        }

        private NetworkObject SetEnemy(EnemyController enemy, int sceneHandle, Vector3 position)
        {
            var instance = Instantiate(enemy.Prefab, position, enemy.Prefab.transform.rotation);

            instance.GetComponent<EnemyTrigger>().SetActive(false);

            EnemyController enemyController = instance.GetComponent<EnemyController>();

            enemyController.FlipDirection(true);

            enemyController.EnterCombat();
            
            InstanceFinder.ServerManager.Spawn(instance, scene: SceneManager.GetScene(sceneHandle));

            return enemyController.NetworkObject;
        }
    }
}