using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers.Server
{
    public class CombatController : NetworkBehaviour
    {
        private static CombatController _instance;

        public static CombatController Singleton
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CombatController>();
                }

                if (_instance == null)
                {
                    if (!InstanceFinder.NetworkManager.IsServerStarted)
                        return null;

                    GameObject controller = new();

                    _instance = controller.AddComponent<CombatController>();

                    InstanceFinder.ServerManager.Spawn(controller);
                }

                return _instance;
            }
        }

        public static readonly string COMBAT_SCENE_NAME = "Combat";

        public void StartCombat(PlayerController player, EnemyController enemy)
        {
            PlayerCharacter playerCharacter = player.GetPlayerCharacter();
            IEnemy enemyCharacter = enemy.Character;

            Debug.Log(playerCharacter.GetName() + " started combat with " + enemyCharacter.GetName());

            player.EnterCombatRpc();

            SceneModule.Singleton.LoadInstance(player.Owner, COMBAT_SCENE_NAME);

            if (player.TryGetComponent<PlayerMovementModule>(out var playerMovement))
            {
                playerMovement.CreateReconcile();
            }
        }
    }
}