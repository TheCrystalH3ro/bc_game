using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using FishNet;
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

                if (_instance == null)
                {
                    if (!InstanceFinder.NetworkManager.IsServerStarted) return null;

                    GameObject controller = new();

                    _instance = controller.AddComponent<CombatServerController>();

                    InstanceFinder.ServerManager.Spawn(controller);
                }

                return _instance;
            }
        }

        public static readonly string COMBAT_SCENE_NAME = "Combat";

        void Start()
        {
            if (!InstanceFinder.IsServerStarted)
                return;

            Initialize();
        }

        public void MoveToCombat(PlayerController player, EnemyController enemy)
        {
            PlayerCharacter playerCharacter = player.GetPlayerCharacter();
            IEnemy enemyCharacter = enemy.Character;

            player.EnterCombatRpc();

            SceneModule.Singleton.LoadInstance(player.Owner, COMBAT_SCENE_NAME);
        }

        public void Initialize()
        {
            List<PlayerController> players = FindObjectsByType<PlayerController>(FindObjectsSortMode.InstanceID).ToList();

            foreach (PlayerController player in players)
            {
                Debug.Log("Combat: " + player.GetPlayerCharacter().GetName());
            }
        }
    }
}