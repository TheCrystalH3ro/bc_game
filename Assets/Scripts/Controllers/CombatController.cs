using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using Assets.Scripts.UI;
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

        private Dictionary<uint, BaseCharacterController> teamA = new();
        private Dictionary<uint, BaseCharacterController> teamB = new();

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
            List<PlayerController> players = FindObjectsByType<PlayerController>(FindObjectsSortMode.InstanceID).ToList();

            foreach (PlayerController player in players)
            {
                teamA.Add(player.GetPlayerCharacter().GetId(), player);
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

            GameObject playerSlot = playerSlots[teamA.Count - 1];

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

            if (isAnsweringQuestion)
            {
                CombatUIController.Singleton.OpenButtonsPanel();
                QuestionAnswered();
            }

            if (!player.Equals(next))
            {
                if (player.Equals(prev))
                {
                    CombatUIController.Singleton.OpenButtonsPanel();

                    if (IsSelectingTarget)
                        TargetSelected();

                    CombatUIController.Singleton.SetButtonsActive(false);
                }

                return;
            }

            CombatUIController.Singleton.SetButtonsActive(true);
        }

        private void OnPlayerSpawned(PlayerController player)
        {
            teamA.Add(player.GetPlayerCharacter().GetId(), player);
            PlacePlayer(player);
            OnDeath(player, teamA);
        }

        private void OnDeath(BaseCharacterController character, Dictionary<uint, BaseCharacterController> team)
        {
            character.GetComponent<HealthModule>().OnDeath.AddListener(() =>
            {
                team.Remove(character.GetId());
                CombatUIController.Singleton.SetCharacterDeath(character);
            });
        }

        private void OnEnemySpawned(EnemyController enemy)
        {
            teamB.Add(enemy.Id, enemy);
            enemy.FlipDirection(true);
            enemy.EnterCombat();
            OnDeath(enemy, teamB);
            CombatUIController.Singleton.LoadCharacter(enemy);
        }

        public void Attack()
        {
            ChooseTarget();
        }

        private void ChooseTarget()
        {
            IsSelectingTarget = true;

            foreach (BaseCharacterController target in teamB.Values)
            {
                target.SetSelectable(true);
            }
        }

        private void TargetSelected()
        {
            IsSelectingTarget = false;

            foreach (BaseCharacterController target in teamB.Values)
            {
                target.SetSelectable(false);
            }
        }

        public void AttackTarget(uint targetId)
        {
            TargetSelected();

            CombatServerController.Singleton.PlayerAttack(targetId);
        }

        private void OnQuestionCreated(FlashCard flashCard)
        {
            isAnsweringQuestion = true;
            StartTimer((int)Math.Floor(flashCard.GetTime()), OnQuestionTimerRunOut);

            CombatUIController.Singleton.SetQuestion(flashCard.GetQuestion(), flashCard.GetAnswers());
        }

        public void AnswerQuestion(uint answerId, int answerIndex)
        {
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

        public void OnAttack(Target attacker, Target enemy)
        {
            BaseCharacterController character = teamA.Values.FirstOrDefault(c => c.Equals(attacker));
            BaseCharacterController target = teamA.Values.FirstOrDefault(c => c.Equals(enemy));

            bool isTargetLeft = true;

            if (character == null)
                character = teamB.Values.FirstOrDefault(c => c.Equals(attacker));

            if (target == null)
            {
                isTargetLeft = false;
                target = teamB.Values.FirstOrDefault(c => c.Equals(enemy));
            }

            if (target == null || attacker == null)
                return;

            CombatUIController.Singleton.SetCharacterAttack(character, target);

            RuntimeAnimatorController hitAnimator = character.GetHitAnimator();
            hitPrefab.GetComponent<Animator>().runtimeAnimatorController = hitAnimator;
            hitPrefab.GetComponent<SpriteRenderer>().flipX = isTargetLeft;

            Instantiate(hitPrefab, target.transform);
        }

        public void AttackMissed(Target enemy, bool isEvaded)
        {
            ShowFloatText(enemy, isEvaded ? "Dodge" : "Missed");
        }

        public void AttackBlocked(Target enemy, bool isPierced)
        {
            ShowFloatText(enemy, isPierced ? "Pierced" : "Blocked");
        }

        public void Stunned(Target enemy)
        {
            ShowFloatText(enemy, "Stunned");
        }

        public void ShowFloatText(Target target, string text)
        {
            BaseCharacterController character = teamA.Values.FirstOrDefault(c => c.Equals(target));

            if (character == null)
                character = teamB.Values.FirstOrDefault(c => c.Equals(target));

            if (character == null)
                return;

            character.DisplayFloatText(text);
        }
    }
}