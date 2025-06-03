using System.Collections.Generic;
using Assets.Scripts.Modules;
using Assets.Scripts.Triggers;
using FishNet;
using FishNet.Connection;
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

            List<PlayerController> players = new() { player };

            List<EnemyController> enemies = new()
            {
                SetEnemy(enemy, sceneHandle, enemySlots[0].transform.position)
            };

            CombatModule.Singleton.StartCombat(players, enemies);
        }

        private EnemyController SetEnemy(EnemyController enemy, int sceneHandle, Vector3 position)
        {
            var instance = Instantiate(enemy.Prefab, position, enemy.Prefab.transform.rotation);

            instance.GetComponent<EnemyTrigger>().SetActive(false);

            EnemyController enemyController = instance.GetComponent<EnemyController>();

            InstanceFinder.ServerManager.Spawn(instance, scene: SceneManager.GetScene(sceneHandle));

            return enemyController;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AttackEnemy(uint enemyId, NetworkConnection sender = null)
        {
            PlayerController player = PlayerController.FindByConnection(sender);

            if (!CombatModule.Singleton.IsValidAttack(player, enemyId))
                return;

            PlayerAttack(player.GetPlayerCharacter().GetId(), enemyId);
            CombatModule.Singleton.AttackEnemy(player, enemyId);
        }

        [ObserversRpc]
        private void PlayerAttack(uint playerId, uint enemyId)
        {
            CombatController.Singleton.PlayerAttack(playerId, enemyId);
        }
    }
}