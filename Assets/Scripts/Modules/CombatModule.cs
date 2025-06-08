using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Modules
{
    public class CombatModule : NetworkBehaviour
    {
        private static CombatModule _instance;

        public static CombatModule Instance
        {
            get
            {
                _instance ??= FindFirstObjectByType<CombatModule>();

                return _instance;
            }
        }

        public int ROUND_TIME = 10;

        private bool isMatchInProgress = false;

        private Dictionary<uint, PlayerController> players;
        private Dictionary<uint, EnemyController> enemies;
        private Dictionary<uint, EnemyController> deadEnemies = new();

        private Coroutine turnTimerCoroutine;

        private readonly SyncVar<BaseCharacterController> CharacterOnTurn = new(new SyncTypeSettings());

        private int currentTurn;

        public UnityEvent<EnemyController, PlayerController> EnemyAttack;

        public static event Action CombatStarted;
        public UnityEvent<CombatModule, List<PlayerController>> CombatEnded;
        public UnityEvent<PlayerController> PlayerEliminated;

        public override void OnStartNetwork()
        {
            if (IsServerInitialized)
                return;

            CombatStarted?.Invoke();
        }

        public void RegisterOnTurnChangeEvent(SyncVar<BaseCharacterController>.OnChanged OnTurnChange)
        {
            CharacterOnTurn.OnChange += OnTurnChange;
        }

        public void UnregisterOnTurnChangeEvent(SyncVar<BaseCharacterController>.OnChanged OnTurnChange)
        {
            CharacterOnTurn.OnChange -= OnTurnChange;
        }

        public void StartCombat(List<PlayerController> players, List<EnemyController> enemies)
        {
            this.players = players.ToDictionary(player => player.GetPlayerCharacter().GetId());
            this.enemies = enemies.ToDictionary(enemy => enemy.Id);
            currentTurn = -1;
            isMatchInProgress = true;
            ChangeTurn();
        }

        private void ChangeTurn()
        {
            StartCoroutine(ChangingTurnProcess());
        }

        private IEnumerator ChangingTurnProcess()
        {
            if (!isMatchInProgress)
                yield break;

            currentTurn = (currentTurn + 1) % (players.Count + enemies.Count);
            CharacterOnTurn.Value = GetCharacterOnTurn();

            if (turnTimerCoroutine != null)
                StopCoroutine(turnTimerCoroutine);

            yield return new WaitForSeconds(1f);

            turnTimerCoroutine = StartCoroutine(StartRoundTimer());

            if (currentTurn >= players.Count)
                EnemyAction();
        }

        private IEnumerator StartRoundTimer()
        {
            int remainingTurnTime = ROUND_TIME;

            while (remainingTurnTime > 0)
            {
                yield return new WaitForSeconds(1f);

                remainingTurnTime--;
            }

            ChangeTurn();
        }

        private BaseCharacterController GetCharacterOnTurn()
        {
            if (currentTurn >= players.Count)
            {
                int enemyIndex = currentTurn - players.Count;
                uint enemyId = enemies.Keys.ToList()[enemyIndex];

                return enemies[enemyId];
            }

            uint playerId = players.Keys.ToList()[currentTurn];
            return players[playerId];
        }

        private bool IsOnTurn(PlayerController player)
        {
            int index = players.Keys.ToList().IndexOf(player.GetPlayerCharacter().GetId());
            return currentTurn == index;
        }

        private bool IsOnTurn(EnemyController enemy)
        {
            int index = (enemies.Keys.ToList().IndexOf(enemy.Id) * -1) + players.Count;
            return currentTurn == index;
        }

        public bool IsValidAttack(PlayerController attacker, uint enemyId)
        {
            if (!players.Values.Contains(attacker))
                return false;

            if (!IsOnTurn(attacker))
                return false;

            if (!enemies.ContainsKey(enemyId))
                return false;

            EnemyController enemy = enemies[enemyId];

            return enemies.ContainsValue(enemy);
        }

        public void AttackEnemy(PlayerController attacker, uint enemyId)
        {
            EnemyController enemy = enemies[enemyId];

            HealthModule enemyHealth = enemy.GetComponent<HealthModule>();
            int enemyHp = enemyHealth.TakeHP(10);

            if (enemyHp <= 0)
                EnemyDeath(enemy);

            ChangeTurn();
        }

        private uint PickTarget(int playerIndex)
        {
            List<uint> targets = players.Keys.ToList();

            if (playerIndex >= 0 && playerIndex < targets.Count)
                return targets[playerIndex];

            playerIndex = UnityEngine.Random.Range(0, targets.Count);

            return targets[playerIndex];
        }

        private void EnemyAction()
        {
            EnemyController enemy = CharacterOnTurn.Value as EnemyController;

            if (enemy == null)
                return;

            int enemyIndex = currentTurn - players.Count;
            uint targetId = PickTarget(enemyIndex);

            AttackPlayer(enemy, targetId);
        }

        private void AttackPlayer(EnemyController attacker, uint playerId)
        {
            PlayerController player = players[playerId];
            EnemyAttack?.Invoke(attacker, player);

            HealthModule playerHealth = player.GetComponent<HealthModule>();
            int playerHp = playerHealth.TakeHP(10);

            if (playerHp <= 0)
                PlayerDeath(player);

            ChangeTurn();
        }

        private void EnemyDeath(EnemyController enemy)
        {
            deadEnemies.Add(enemy.Id, enemy);
            enemies.Remove(enemy.Id);

            if (enemies.Count > 0)
                return;

            EndCombat(true);
        }

        private void PlayerDeath(PlayerController player)
        {
            players.Remove(player.GetPlayerCharacter().GetId());
            PlayerEliminated?.Invoke(player);

            if (players.Count > 0)
                return;

            EndCombat(false);
        }

        private void EndCombat(bool playerWon)
        {
            isMatchInProgress = false;

            CombatEnded?.Invoke(this, players.Values.ToList());

            StopCoroutine(turnTimerCoroutine);

            players = new();
            enemies = new();
        }
    }
}