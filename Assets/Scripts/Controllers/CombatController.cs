using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using Assets.Scripts.UI.Controllers;
using FishNet;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class CombatController : MonoBehaviour
    {
        private static CombatController _instance;

        public static CombatController Singleton
        {
            get
            {
                _instance ??= FindFirstObjectByType<CombatController>();

                return _instance;
            }
        }

        [SerializeField] private GameObject hitPrefab;

        private Dictionary<uint, PlayerController> players = new();
        private Dictionary<uint, EnemyController> enemies = new();

        private Coroutine roundTimerCoroutine = null;
        private bool IsSelectingTarget = false;
        private bool isAnsweringQuestion = false;
        private int selectedAnswer;
        private BaseCharacterController characterOnTurn;

        void OnEnable()
        {
            PlayerController.PlayerSpawned += OnPlayerSpawned;
            EnemyController.EnemySpawned += OnEnemySpawned;
            CombatModule.CombatStarted += StartCombat;
            CombatModule.QuestionCreated += OnQuestionCreated;
            CombatModule.TimerStarted += OnTimerStarted;
        }

        void OnDisable()
        {
            PlayerController.PlayerSpawned -= OnPlayerSpawned;
            EnemyController.EnemySpawned -= OnEnemySpawned;
            CombatModule.CombatStarted -= StartCombat;
            CombatModule.QuestionCreated -= OnQuestionCreated;
            CombatModule.TimerStarted -= OnTimerStarted;
        }

        void Start()
        {
            if (InstanceFinder.IsServerStarted)
                return;

            Initialize();
        }

        public void Initialize()
        {
            players = FindObjectsByType<PlayerController>(FindObjectsSortMode.InstanceID).ToDictionary(player => player.GetPlayerCharacter().GetId());

            foreach (PlayerController player in players.Values)
            {
                PlacePlayer(player);
            }
        }

        public void StartCombat()
        {
            CombatUIController.Singleton.SetRoundTime(CombatModule.Instance.ROUND_TIME);
            CombatModule.Instance.RegisterOnTurnChangeEvent(TurnChanged);
        }

        private void PlacePlayer(PlayerController player)
        {
            player.EnterCombat();

            GameObject[] playerSlots = GameObject.FindGameObjectsWithTag("PlayerSlot").OrderBy(slot => slot.name).ToArray();

            GameObject playerSlot = playerSlots[players.Count - 1];

            player.gameObject.transform.position = playerSlot.transform.position;

            player.FlipDirection(false);

            CombatUIController.Singleton.LoadCharacter(player);
        }

        private IEnumerator RoundTimer(int maxTime, Action onTimerRunOut = null)
        {
            int remainingTurnTime = maxTime;

            while (remainingTurnTime >= 0)
            {
                CombatUIController.Singleton.ChangeRoundTime(remainingTurnTime);

                yield return new WaitForSeconds(1f);

                remainingTurnTime--;
            }

            onTimerRunOut?.Invoke();
        }

        private void StartTimer(int maxTime, Action onTimerRunOut = null)
        {
            if (roundTimerCoroutine != null)
                StopCoroutine(roundTimerCoroutine);

            CombatUIController.Singleton.SetRoundTime(maxTime);
            roundTimerCoroutine = StartCoroutine(RoundTimer(maxTime, onTimerRunOut));
        }

        private void OnTimerStarted(int time)
        {
            if (PlayerController.Singleton.Equals(characterOnTurn))
                return;

            StartTimer(time);
        }

        private void TurnChanged(BaseCharacterController prev, BaseCharacterController next, bool asServer)
        {
            characterOnTurn = next;

            StartTimer(CombatModule.Instance.ROUND_TIME);

            CombatUIController.Singleton.SetCharacterTurn(next);

            PlayerController player = PlayerController.Singleton;

            if (!player.Equals(next))
            {
                if (player.Equals(prev))
                {
                    CombatUIController.Singleton.OpenButtonsPanel();

                    if (IsSelectingTarget)
                        TargetSelected();

                    if (isAnsweringQuestion)
                        QuestionAnswered();

                    CombatUIController.Singleton.SetButtonsActive(false);
                }

                return;
            }

            CombatUIController.Singleton.SetButtonsActive(true);
        }

        private void OnPlayerSpawned(PlayerController player)
        {
            players.Add(player.GetPlayerCharacter().GetId(), player);
            PlacePlayer(player);
            OnPlayerDeath(player);
        }

        private void OnPlayerDeath(PlayerController player)
        {
            player.GetComponent<HealthModule>().OnDeath.AddListener(() =>
            {
                players.Remove(player.GetPlayerCharacter().GetId());
                CombatUIController.Singleton.SetCharacterDeath(player);
            });
        }

        private void OnEnemySpawned(EnemyController enemy)
        {
            enemies.Add(enemy.Id, enemy);
            enemy.FlipDirection(true);
            enemy.EnterCombat();
            OnEnemyDeath(enemy);
            CombatUIController.Singleton.LoadCharacter(enemy);
        }

        private void OnEnemyDeath(EnemyController enemy)
        {
            enemy.GetComponent<HealthModule>().OnDeath.AddListener(() =>
            {
                enemies.Remove(enemy.Id);
                CombatUIController.Singleton.SetCharacterDeath(enemy);
            });
        }

        public void Attack()
        {
            ChooseTarget();
        }

        private void ChooseTarget()
        {
            IsSelectingTarget = true;

            foreach (EnemyController enemy in enemies.Values)
            {
                enemy.SetSelectable(true);
            }
        }

        private void TargetSelected()
        {
            IsSelectingTarget = false;

            foreach (EnemyController enemy in enemies.Values)
            {
                enemy.SetSelectable(false);
            }
        }

        public void AttackTarget(uint targetId)
        {
            TargetSelected();

            CombatServerController.Singleton.AttackEnemy(targetId);
        }

        private void OnQuestionCreated(FlashCard flashCard)
        {
            isAnsweringQuestion = true;
            StartTimer((int) Math.Floor(flashCard.GetTime()), OnQuestionTimerRunOut);

            CombatUIController.Singleton.SetQuestion(flashCard.GetQuestion(), flashCard.GetAnswers());
        }

        public void AnswerQuestion(uint answerId, int answerIndex)
        {
            if (!characterOnTurn.Equals(PlayerController.Singleton))
                return;

            CombatUIController.Singleton.AnswerSubmitted();

            selectedAnswer = answerIndex;

            CombatServerController.Singleton.AnswerQuestion(answerId);
        }

        private void QuestionAnswered()
        {
            isAnsweringQuestion = false;

            if (selectedAnswer < 0)
                return;

            CombatUIController.Singleton.ClearAnswerResult(selectedAnswer);

            selectedAnswer = -1;
        }

        private void OnQuestionTimerRunOut()
        {
            selectedAnswer = -1;
        }

        public void AnswerResultReceived(bool isCorrect)
        {
            if (selectedAnswer < 0)
                return;

            CombatUIController.Singleton.SetAnswerResult(selectedAnswer, isCorrect);
        }

        public void PlayerAttack(uint playerId, uint enemyId)
        {
            PlayerController player = players[playerId];
            EnemyController enemy = enemies[enemyId];

            if (enemy == null || player == null)
                return;

            CombatUIController.Singleton.SetCharacterAttack(player, enemy);

            RuntimeAnimatorController hitAnimator = ClassAnimationController.Singleton.GetCharacterAttackAnimatorController(player.GetPlayerCharacter().GetPlayerClass());
            hitPrefab.GetComponent<Animator>().runtimeAnimatorController = hitAnimator;
            hitPrefab.GetComponent<SpriteRenderer>().flipX = false;

            Instantiate(hitPrefab, enemy.transform);
        }

        public void EnemyAttack(uint enemyId, uint playerId)
        {
            EnemyController enemy = enemies[enemyId];
            PlayerController player = players[playerId];

            if (enemy == null || player == null)
                return;

            CombatUIController.Singleton.SetCharacterAttack(enemy, player);

            RuntimeAnimatorController hitAnimator = enemy.GetHitAnimator();
            hitPrefab.GetComponent<Animator>().runtimeAnimatorController = hitAnimator;
            hitPrefab.GetComponent<SpriteRenderer>().flipX = true;

            Instantiate(hitPrefab, player.transform);
        }
    }
}