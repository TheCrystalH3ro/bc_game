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

        private Dictionary<BaseCharacterController, CombatModule> instances = new();

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
                instances.Remove(character);

                character.OnAttack.RemoveListener(OnAttack);
                character.OnStun.RemoveListener(OnStun);

                PlayerController player = character as PlayerController;

                if (player == null)
                    continue;

                connections.Add(player.Owner);

                player.LeaveCombatRpc();
            }

            if(connections.Count > 0)
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

                player.OnAttack.AddListener(OnAttack);
                player.OnStun.AddListener(OnStun);

                EnemyController enemyInstance = SetEnemy(enemy, sceneHandle, enemySlots[enemies.Count].transform.position);
                enemies.Add(enemyInstance);

                enemyInstance.OnAttack.AddListener(OnAttack);
                enemyInstance.OnStun.AddListener(OnStun);

                instances.Add(player, combatModule);
                instances.Add(enemyInstance, combatModule);
            }

            combatModule.StartCombat(players, enemies);

            combatModule.CombatEnded.AddListener(OnCombatEnded);
            combatModule.AttackFailed.AddListener(OnAttackFailed);
            combatModule.AttackBlocked.AddListener(OnAttackBlocked);
            combatModule.PlayerEliminated.AddListener(OnPlayerDeath);
            combatModule.EnemyEliminated.AddListener(OnEnemyDeath);
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

        public CombatModule GetInstance(BaseCharacterController character)
        {
            return instances.FirstOrDefault(kvp => kvp.Key.Equals(character)).Value;
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayerAttack(uint targetId, NetworkConnection sender = null)
        {
            PlayerController player = PlayerController.FindByConnection(sender);
            CombatModule combatModule = instances[player];

            if (!combatModule.IsValidAttack(player, targetId))
                return;

            combatModule.Attack(player, targetId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AnswerQuestion(uint answerId, NetworkConnection sender = null)
        {
            PlayerController player = PlayerController.FindByConnection(sender);
            CombatModule combatModule = instances[player];

            if (!combatModule.IsValidAnswer(player))
                return;

            BaseCharacterController target = combatModule.GetCurrentTarget();

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
        private void Attack(Target attacker, Target target)
        {
            CombatController.Singleton.OnAttack(attacker, target);
        }

        private void OnAttackFailed(BaseCharacterController target, bool isEvaded)
        {
            AttackMissed(target.ToTarget(), isEvaded);
        }

        [ObserversRpc]
        private void AttackMissed(Target target, bool isEvaded)
        {
            CombatController.Singleton.AttackMissed(target, isEvaded);
        }

        private void OnAttackBlocked(BaseCharacterController target, bool isPierced)
        {
            AttackBlocked(target.ToTarget(), isPierced);
        }

        [ObserversRpc]
        private void AttackBlocked(Target target, bool isPierced)
        {
            CombatController.Singleton.AttackBlocked(target, isPierced);
        }

        private void OnStun(BaseCharacterController target)
        {
            Stunned(target.ToTarget());
        }

        [ObserversRpc]
        private void Stunned(Target target)
        {
            CombatController.Singleton.Stunned(target);
        }

        private void OnAttack(BaseCharacterController attacker, BaseCharacterController target)
        {
            Attack(attacker.ToTarget(), target.ToTarget());
        }

        public void OnPlayerDeath(PlayerController player)
        {
            player.OnAttack.RemoveListener(OnAttack);
            player.OnStun.RemoveListener(OnStun);

            StartCoroutine(PlayerDied(player));
        }

        public void OnEnemyDeath(EnemyController enemy)
        {
            enemy.OnAttack.RemoveListener(OnAttack);
            enemy.OnStun.RemoveListener(OnStun);
        }

        private IEnumerator PlayerDied(PlayerController player)
        {
            yield return new WaitForSeconds(1f);

            MoveOutOfCombat(player, SceneModule.DEFAULT_SCENE_NAME);

            player.RespawnPlayer();
        }

        private void OnCombatEnded(CombatModule combatModule, List<BaseCharacterController> teamA, List<BaseCharacterController> teamB)
        {
            if (teamA.Count > 0)
            {
                MoveOutOfCombat(teamA);
            }

            if (teamA.Count > 0)
            {
                MoveOutOfCombat(teamB);
            }

            combatModule.CombatEnded.RemoveListener(OnCombatEnded);
            combatModule.AttackFailed.RemoveListener(OnAttackFailed);
            combatModule.AttackBlocked.RemoveListener(OnAttackBlocked);
            combatModule.PlayerEliminated.RemoveListener(OnPlayerDeath);
            combatModule.QuestionAnswered.RemoveListener(OnQuestionAnswered);
        }
    }
}