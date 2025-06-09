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

        public void MoveOutOfCombat(BaseCharacterController character, string newScene = null)
        {
            MoveOutOfCombat(new List<BaseCharacterController>() { character }, newScene);
        }

        public void MoveOutOfCombat(List<BaseCharacterController> characters, string newScene = null)
        {
            List<NetworkConnection> connections = new();

            foreach (BaseCharacterController character in characters)
            {
                PlayerController player = character as PlayerController;

                if (player == null)
                    continue;

                connections.Add(player.Owner);

                instances.Remove(player.Owner);
                player.LeaveCombatRpc();
            }

            SceneModule.Singleton.LeaveInstance(connections, newScene);
        }

        public void Initialize(List<NetworkConnection> connections, EnemyController enemy, int sceneHandle)
        {
            GameObject[] enemySlots = GameObject.FindGameObjectsWithTag("EnemySlot").OrderBy(slot => slot.name).ToArray();

            List<BaseCharacterController> players = new();
            List<BaseCharacterController> enemies = new();

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
            combatModule.PlayerAttackFailed.AddListener(OnPlayerAttackFailed);
            combatModule.EnemyAttackFailed.AddListener(OnEnemyAttackFailed);
            combatModule.PlayerEliminated.AddListener(OnPlayerDeath);
            combatModule.QuestionAnswered.AddListener(OnQuestionAnswered);
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
        public void Attack(uint targetId, NetworkConnection sender = null)
        {
            PlayerController player = PlayerController.FindByConnection(sender);
            CombatModule combatModule = instances[player.Owner];

            if (!combatModule.IsValidAttack(player, targetId))
                return;

            combatModule.Attack(player, targetId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AnswerQuestion(uint answerId, NetworkConnection sender = null)
        {
            PlayerController player = PlayerController.FindByConnection(sender);
            CombatModule combatModule = instances[player.Owner];

            if (!combatModule.IsValidAnswer(player))
                return;

            BaseCharacterController target = combatModule.GetCurrentTarget();

            PlayerAttack(player.GetPlayerCharacter().GetId(), target.GetId());
            combatModule.AnswerQuestion(player, answerId);
        }

        private void OnQuestionAnswered(PlayerController player, bool isCorrect)
        {
            NotifyQuestionAnswer(player.Owner, isCorrect);
        }

        [TargetRpc]
        private void NotifyQuestionAnswer(NetworkConnection connection, bool isCorrect)
        {
            CombatController.Singleton.AnswerResultReceived(isCorrect);
        }

        [ObserversRpc]
        private void PlayerAttack(uint playerId, uint targetId)
        {
            CombatController.Singleton.PlayerAttack(playerId, targetId);
        }

        private void OnPlayerAttackFailed(BaseCharacterController target)
        {
            PlayerAttackMissed(target.GetId());
        }

        [ObserversRpc]
        private void PlayerAttackMissed(uint targetId)
        {
            CombatController.Singleton.PlayerAttackMissed(targetId);
        }

        private void OnEnemyAttackFailed(BaseCharacterController target)
        {
            EnemyAttackMissed(target.GetId());
        }

        [ObserversRpc]
        private void EnemyAttackMissed(uint targetId)
        {
            CombatController.Singleton.EnemyAttackMissed(targetId);
        }

        private void OnEnemyAttack(EnemyController enemy, BaseCharacterController target)
        {
            EnemyAttack(enemy.Id, target.GetId());
        }

        [ObserversRpc]
        private void EnemyAttack(uint enemyId, uint targetId)
        {
            CombatController.Singleton.EnemyAttack(enemyId, targetId);
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

        private void OnCombatEnded(CombatModule combatModule, List<BaseCharacterController> players)
        {
            if (players.Count > 0)
            {
                MoveOutOfCombat(players);
            }

            combatModule.CombatEnded.RemoveListener(OnCombatEnded);
            combatModule.EnemyAttack.RemoveListener(OnEnemyAttack);
            combatModule.PlayerEliminated.RemoveListener(OnPlayerDeath);
            combatModule.QuestionAnswered.RemoveListener(OnQuestionAnswered);
        }
    }
}