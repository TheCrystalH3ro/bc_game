using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using Assets.Scripts.Triggers;
using FishNet;
using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
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

        [SerializeField] private GameObject combatModulePrefab;

        private Dictionary<NetworkConnection, CombatModule> instances = new();

        public static readonly string COMBAT_SCENE_NAME = "Combat"; 

        public void MoveToCombat(PlayerController player, EnemyController enemy)
        {
            List<NetworkConnection> connections;
            IParty party = player.GetParty();

            if (party != null)
            {
                connections = party.GetConnectionsInScene(player.gameObject.scene);
            }
            else
            {
                connections = new() { player.Owner };
            }

            foreach (NetworkConnection connection in connections)
            {
                PlayerController member = connection.FirstObject.GetComponent<PlayerController>();
                member.EnterCombatRpc();
            }

            SceneModule.Singleton.LoadInstance(connections, COMBAT_SCENE_NAME, instanceLoadEvent: (sceneHandle) =>
            {
                Initialize(connections, enemy, sceneHandle);
            });
        }

        public void MoveOutOfCombat(PlayerController player, string newScene = null)
        {
            MoveOutOfCombat(new List<PlayerController>() { player }, newScene);
        }

        public void MoveOutOfCombat(List<PlayerController> players, string newScene = null)
        {
            List<NetworkConnection> connections = new();

            foreach (PlayerController player in players)
            {
                connections.Add(player.Owner);

                instances.Remove(player.Owner);
                player.LeaveCombatRpc();
            }

            Debug.Log("Removing " + connections.Count + " player(s) from the instance");

            SceneModule.Singleton.LeaveInstance(connections, newScene);
        }

        public void Initialize(List<NetworkConnection> connections, EnemyController enemy, int sceneHandle)
        {
            GameObject[] enemySlots = GameObject.FindGameObjectsWithTag("EnemySlot").OrderBy(slot => slot.name).ToArray();

            List<PlayerController> players = new();
            List<EnemyController> enemies = new();

            var combatModuleInstance = Instantiate(combatModulePrefab);
            InstanceFinder.ServerManager.Spawn(combatModuleInstance, scene: SceneManager.GetScene(sceneHandle));

            CombatModule combatModule = combatModuleInstance.GetComponent<CombatModule>();

            foreach (NetworkConnection connection in connections)
            {
                PlayerController player = PlayerController.FindByConnection(connection);
                players.Add(player);

                EnemyController enemyInstance = SetEnemy(enemy, sceneHandle, enemySlots[enemies.Count].transform.position);
                enemies.Add(enemyInstance);

                instances.Add(player.Owner, combatModule);
            }

            combatModule.StartCombat(players, enemies);

            combatModule.EnemyAttack.AddListener(OnEnemyAttack);
            combatModule.CombatEnded.AddListener(OnCombatEnded);
            combatModule.PlayerEliminated.AddListener(OnPlayerDeath);
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
            CombatModule combatModule = instances[player.Owner];

            if (!combatModule.IsValidAttack(player, enemyId))
                return;

            PlayerAttack(player.GetPlayerCharacter().GetId(), enemyId);
            combatModule.AttackEnemy(player, enemyId);
        }

        [ObserversRpc]
        private void PlayerAttack(uint playerId, uint enemyId)
        {
            CombatController.Singleton.PlayerAttack(playerId, enemyId);
        }

        private void OnEnemyAttack(EnemyController enemy, PlayerController player)
        {
            EnemyAttack(enemy.Id, player.GetPlayerCharacter().GetId());
        }

        [ObserversRpc]
        private void EnemyAttack(uint enemyId, uint playerId)
        {
            CombatController.Singleton.EnemyAttack(enemyId, playerId);
        }

        public void OnPlayerDeath(PlayerController player)
        {
            StartCoroutine(PlayerDied(player));
        }

        private IEnumerator PlayerDied(PlayerController player)
        {
            yield return new WaitForSeconds(1f);

            MoveOutOfCombat(player, SceneModule.DEFAULT_SCENE_NAME);

            player.RespawnPlayer();
        }

        private void OnCombatEnded(CombatModule combatModule, List<PlayerController> players)
        {
            if (players.Count > 0)
            {
                MoveOutOfCombat(players);
            }

            combatModule.CombatEnded.RemoveListener(OnCombatEnded);
            combatModule.EnemyAttack.RemoveListener(OnEnemyAttack);
            combatModule.PlayerEliminated.RemoveListener(OnPlayerDeath);
        }
    }
}