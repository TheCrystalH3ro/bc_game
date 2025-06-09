using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using FishNet.Connection;
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

        private bool isRoundTimeRunning = false;
        private bool isQuestionTimeRunning = false;

        private Dictionary<uint, PlayerController> players;
        private Dictionary<uint, EnemyController> enemies;
        private Dictionary<uint, EnemyController> deadEnemies = new();

        private Coroutine turnTimerCoroutine;
        private Coroutine questionTimerCoroutine;

        private float remainingQuestionTime;

        private readonly SyncVar<BaseCharacterController> CharacterOnTurn = new(new SyncTypeSettings());
        private bool actionFinished = false;

        private int currentTurn;

        private FlashCard currentQuestion;
        private BaseCharacterController currentTarget;

        public UnityEvent<EnemyController, PlayerController> EnemyAttack;

        public static event Action CombatStarted;
        public static event Action<FlashCard> QuestionCreated;
        public static event Action<int> TimerStarted;
        public UnityEvent<PlayerController, bool> QuestionAnswered;
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
            ChangeTurn();
        }

        private void ChangeTurn(float waitTime = 0)
        {
            isRoundTimeRunning = true;
            StartCoroutine(ChangingTurnProcess(waitTime));
        }

        private IEnumerator ChangingTurnProcess(float waitTime = 0)
        {
            yield return new WaitForSeconds(waitTime);

            currentTurn = (currentTurn + 1) % (players.Count + enemies.Count);
            CharacterOnTurn.Value = GetCharacterOnTurn();
            actionFinished = false;

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

                if (!isRoundTimeRunning)
                    yield break;
            }

            ChangeTurn();
        }

        private void StopRoundTimer()
        {
            isRoundTimeRunning = false;
            StopCoroutine(turnTimerCoroutine);
        }

        private IEnumerator StartQuestionTimer()
        {
            remainingQuestionTime = currentQuestion.GetTime();
            OnTimerStarted((int) Math.Floor(remainingQuestionTime));

            while (remainingQuestionTime > 0)
            {
                yield return new WaitForSeconds(1f);

                remainingQuestionTime--;

                if (!isQuestionTimeRunning)
                    yield break;
            }

            isQuestionTimeRunning = false;
            QuestionAnswerFailed();
        }

        private void StopQuestionTimer()
        {
            isQuestionTimeRunning = false;

            if (questionTimerCoroutine != null)
                StopCoroutine(questionTimerCoroutine);
        }

        [ObserversRpc]
        private void OnTimerStarted(int time)
        {
            TimerStarted?.Invoke(time);
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

        public BaseCharacterController GetCurrentTarget()
        {
            return currentTarget;
        }

        public bool IsValidAttack(PlayerController attacker, uint enemyId)
        {
            if (!players.Values.Contains(attacker))
                return false;

            if (!IsOnTurn(attacker))
                return false;

            if (actionFinished)
                return false;

            if (!enemies.ContainsKey(enemyId))
                return false;

            EnemyController enemy = enemies[enemyId];

            return enemies.ContainsValue(enemy);
        }

        public bool IsValidAnswer(PlayerController player)
        {
            if (!players.Values.Contains(player))
                return false;

            if (!IsOnTurn(player))
                return false;

            if (currentQuestion == null || currentTarget == null || currentTarget is not EnemyController)
                return false;

            return true;
        }

        public void AttackEnemy(PlayerController attacker, uint enemyId)
        {
            actionFinished = true;

            EnemyController enemy = enemies[enemyId];

            currentTarget = enemy;

            StopRoundTimer();

            GenerateQuestion();
        }

        private void GenerateQuestion()
        {
            currentQuestion = FlashCardModule.Singleton.GetFlashCard();

            SendQuestion(CharacterOnTurn.Value.Owner, currentQuestion);

            isQuestionTimeRunning = true;
            questionTimerCoroutine = StartCoroutine(StartQuestionTimer());
        }

        [TargetRpc]
        private void SendQuestion(NetworkConnection client, FlashCard flashCard)
        {
            QuestionCreated?.Invoke(flashCard);
        } 

        public void AnswerQuestion(PlayerController player, uint answer)
        {
            StopQuestionTimer();

            if (!currentQuestion.IsCorrectAnswer(answer))
            {
                QuestionAnswerFailed();
                return;
            }

            QuestionAnswered?.Invoke(player, true);

            EnemyController enemy = currentTarget as EnemyController;

            if (enemy == null)
                return;

            int damage = player.GetDamage(currentQuestion, remainingQuestionTime);

            HealthModule enemyHealth = enemy.GetComponent<HealthModule>();
            int enemyHp = enemyHealth.TakeHP(damage);

            if (enemyHp <= 0)
                EnemyDeath(enemy);

            currentQuestion = null;
            currentTarget = null;

            ChangeTurn(1.5f);
        }

        private void QuestionAnswerFailed()
        {
            QuestionAnswered?.Invoke(CharacterOnTurn.Value as PlayerController, false);

            currentQuestion = null;
            currentTarget = null;
            ChangeTurn(1.5f);
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
            isRoundTimeRunning = false;

            CombatEnded?.Invoke(this, players.Values.ToList());

            StopCoroutine(turnTimerCoroutine);

            players = new();
            enemies = new();
        }
    }
}