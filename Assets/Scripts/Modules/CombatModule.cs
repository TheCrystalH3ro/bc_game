using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
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

        private Dictionary<uint, BaseCharacterController> teamA;
        private Dictionary<uint, BaseCharacterController> teamB;
        private Dictionary<uint, EnemyController> deadEnemies = new();

        private Coroutine turnTimerCoroutine;
        private Coroutine questionTimerCoroutine;

        private float remainingQuestionTime;

        private readonly SyncVar<BaseCharacterController> CharacterOnTurn = new(new SyncTypeSettings());
        private bool actionFinished = false;

        private int currentTurn;

        private FlashCard currentQuestion;
        private BaseCharacterController currentTarget;

        private Answer attackerAnswer = null;
        private Answer targetAnswer = null;
        public static event Action CombatStarted;
        public static event Action<FlashCard> QuestionCreated;
        public static event Action<int> TimerStarted;
        public UnityEvent<BaseCharacterController, bool> AttackFailed;
        public UnityEvent<BaseCharacterController, bool> AttackBlocked;
        public UnityEvent<PlayerController, bool> QuestionAnswered;
        public UnityEvent<CombatModule, List<BaseCharacterController>, List<BaseCharacterController>> CombatEnded;
        public UnityEvent<PlayerController> PlayerEliminated;
        public UnityEvent<EnemyController> EnemyEliminated;

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

        private Dictionary<uint, BaseCharacterController> CreateTeam(List<BaseCharacterController> characters)
        {
            return characters.ToDictionary(character =>
            {
                PlayerController player = character as PlayerController;
                if (player != null)
                    return player.GetPlayerCharacter().GetId();

                EnemyController enemy = character as EnemyController;
                return enemy.Id;
            });
        }

        public Dictionary<uint, BaseCharacterController> GetTeam(BaseCharacterController character)
        {
            return IsInTeam(teamA, character) ? teamA : teamB;
        }

        public void StartCombat(List<BaseCharacterController> charactersA, List<BaseCharacterController> charactersB)
        {
            teamA = CreateTeam(charactersA);
            teamB = CreateTeam(charactersB);
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

            if ((teamA.Count + teamB.Count) == 0)
                yield break;

            currentTurn = (currentTurn + 1) % (teamA.Count + teamB.Count);
            CharacterOnTurn.Value = GetCharacterOnTurn();
            actionFinished = false;

            if (turnTimerCoroutine != null)
                StopCoroutine(turnTimerCoroutine);

            yield return new WaitForSeconds(1f);

            turnTimerCoroutine = StartCoroutine(StartRoundTimer());

            if (CharacterOnTurn.Value is EnemyController)
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
            QuestionTimerRunout();
        }

        private void StopQuestionTimer()
        {
            isQuestionTimeRunning = false;

            if (questionTimerCoroutine != null)
                StopCoroutine(questionTimerCoroutine);
        }

        private void QuestionTimerRunout()
        {
            attackerAnswer ??= new(currentQuestion, false, 0);
            targetAnswer ??= new(currentQuestion, false, 0);

            EvaluateAnswers();
        }

        [ObserversRpc]
        private void OnTimerStarted(int time)
        {
            TimerStarted?.Invoke(time);
        }

        private BaseCharacterController GetCharacterOnTurn()
        {
            uint characterId;

            if (currentTurn >= teamA.Count)
            {
                int index = currentTurn - teamA.Count;
                characterId = teamB.Keys.ToList()[index];

                return teamB[characterId];
            }

            characterId = teamA.Keys.ToList()[currentTurn];
            return teamA[characterId];
        }

        private bool IsOnTurn(BaseCharacterController character)
        {
            return character.Equals(CharacterOnTurn.Value);
        }

        public BaseCharacterController GetCurrentTarget()
        {
            return currentTarget;
        }

        private bool IsInTeam(Dictionary<uint, BaseCharacterController> team, BaseCharacterController character)
        {
            return team.Values.Contains(character);
        }

        public bool IsValidAttack(BaseCharacterController attacker, uint targetId)
        {
            if (!IsInTeam(teamA, attacker) && !IsInTeam(teamB, attacker))
                return false;

            if (!IsOnTurn(attacker))
                return false;

            if (actionFinished)
                return false;

            if (IsInTeam(teamA, attacker))
            {
                if (!teamB.ContainsKey(targetId))
                    return false;

                return true;
            }

            if (!teamA.ContainsKey(targetId))
                return false;

            return true;
        }

        public bool IsValidAnswer(BaseCharacterController character)
        {
            if (currentQuestion == null || currentTarget == null)
                return false;

            if (!IsOnTurn(character) && !character.Equals(currentTarget))
                return false;
            
            return true;
        }

        public void Attack(BaseCharacterController attacker, uint targetId)
        {
            if (attacker.IsStunned())
            {
                attacker.UseStun();
                ChangeTurn(1.5f);
                
                return;
            }

            actionFinished = true;

            BaseCharacterController target = IsInTeam(teamA, attacker) ? teamB[targetId] : teamA[targetId];

            currentTarget = target;

            StopRoundTimer();

            GenerateQuestion();
        }

        private void GenerateQuestion()
        {
            currentQuestion = FlashCardModule.Singleton.GetFlashCard();

            SendQuestion(CharacterOnTurn.Value);
            SendQuestion(currentTarget);

            isQuestionTimeRunning = true;
            questionTimerCoroutine = StartCoroutine(StartQuestionTimer());
        }

        private void SendQuestion(BaseCharacterController character)
        {
            EnemyController enemy = character as EnemyController;

            if (enemy != null)
            {
                StartCoroutine(EnemyAnswerQuestion(enemy));
                return;
            }

            SendQuestionToPlayer(character.Owner, currentQuestion);
        }

        [TargetRpc]
        private void SendQuestionToPlayer(NetworkConnection client, FlashCard flashCard)
        {
            QuestionCreated?.Invoke(flashCard);
        }

        public void AnswerQuestion(BaseCharacterController character, uint answer)
        {
            if (!IsValidAnswer(character))
                return;

            bool isCorrect = currentQuestion.IsCorrectAnswer(answer);

            PlayerController player = character as PlayerController;

            if (player != null)
                QuestionAnswered?.Invoke(player, isCorrect);

            Answer characterAnswer = new(currentQuestion, isCorrect, remainingQuestionTime);

            if (IsOnTurn(character))
            {
                attackerAnswer = characterAnswer;

                if (targetAnswer != null)
                    EvaluateAnswers();

                return;
            }

            targetAnswer = characterAnswer;

            if (attackerAnswer != null)
                EvaluateAnswers();
        }

        private void QuestionAnswerFailed(bool isEvaded)
        {
            AttackFailed?.Invoke(currentTarget, isEvaded);
        }

        private void EvaluateAnswers()
        {
            StopQuestionTimer();

            BaseCharacterController attacker = CharacterOnTurn.Value;
            BaseCharacterController target = currentTarget;

            int damage = attackerAnswer.IsCorrect ? attacker.GetDamage(currentQuestion, attackerAnswer.RemainingTime) : 0;

            float defense = 1f;

            if (targetAnswer.IsCorrect)
            {
                defense = 0.85f;

                if (attackerAnswer.RemainingTime < targetAnswer.RemainingTime)
                    defense = target.GetDefense(currentQuestion, targetAnswer.RemainingTime);
            }

            damage = attacker.BeforeDamage(damage, target, attackerAnswer, targetAnswer);
            defense = target.BeforeDefense(defense, attacker, targetAnswer, attackerAnswer);

            int finalDamage = Mathf.FloorToInt(damage * defense);
            int dmgToDeal = attacker.BeforeFinalDamage(finalDamage, damage, defense, target, attackerAnswer, targetAnswer);


            if (defense < 1 && dmgToDeal != 0)
            {
                if (dmgToDeal > finalDamage)
                    AttackBlocked?.Invoke(target, true);
                else
                    AttackBlocked?.Invoke(target, false);
            }

            if (dmgToDeal == 0)
            {
                attacker.AfterDamage(target, attackerAnswer, targetAnswer);
                target.AfterDefense(attacker, targetAnswer, attackerAnswer);

                QuestionAnswerFailed(damage != 0);
                EndQuestion();
                return;
            }

            int targetHp = attacker.DoAttack(target, dmgToDeal);

            attacker.AfterDamage(target, attackerAnswer, targetAnswer);
            target.AfterDefense(attacker, targetAnswer, attackerAnswer);

            if (targetHp <= 0)
                Death(target);

            EndQuestion();
        }

        private void EndQuestion()
        {
            currentQuestion = null;
            currentTarget = null;

            attackerAnswer = null;
            targetAnswer = null;

            ChangeTurn(1.5f);
        }

        private IEnumerator EnemyAnswerQuestion(EnemyController enemy)
        {
            if (currentQuestion == null)
                yield break;

            FlashCard question = currentQuestion;

            float answerTime = enemy.GetThinkingTime(question);
            uint answer = enemy.GetQuestionAnswer(question);

            yield return new WaitForSeconds(answerTime);

            AnswerQuestion(enemy, answer);

            enemy.Learn(question, question.IsCorrectAnswer(answer), answerTime);
        }

        private uint PickTarget(Dictionary<uint, BaseCharacterController> team, int index)
        {
            List<uint> targets = team.Keys.ToList();

            if (index >= 0 && index < targets.Count)
                return targets[index];

            index = UnityEngine.Random.Range(0, targets.Count);

            return targets[index];
        }

        private void EnemyAction()
        {
            EnemyController enemy = CharacterOnTurn.Value as EnemyController;

            if (enemy == null)
                return;

            Dictionary<uint, BaseCharacterController> team = IsInTeam(teamA, enemy) ? teamB : teamA;

            int enemyIndex = (currentTurn >= teamA.Count) ? (currentTurn - teamA.Count) : currentTurn;
            uint targetId = PickTarget(team, enemyIndex);

            Attack(enemy, targetId);
        }

        private void Death(BaseCharacterController character)
        {
            Dictionary<uint, BaseCharacterController> team = IsInTeam(teamA, character) ? teamA : teamB;

            PlayerController player = character as PlayerController;

            bool playerWon = true;

            if (player != null)
            {
                PlayerEliminated?.Invoke(player);
                playerWon = false;
            }
            else
            {
                EnemyController enemy = character as EnemyController;
                EnemyEliminated?.Invoke(enemy);
                deadEnemies.Add(enemy.Id, enemy);
            }

            team.Remove(character.GetId());

            if (team.Count > 0)
                return;

            Dictionary<uint, BaseCharacterController> winningTeam = !IsInTeam(teamA, character) ? teamA : teamB;

            EndCombat(winningTeam, team, playerWon);
        }

        private void EndCombat(Dictionary<uint, BaseCharacterController> winningTeam, Dictionary<uint, BaseCharacterController> losingTeam, bool playerWon)
        {
            isRoundTimeRunning = false;

            if (playerWon)
                CombatEnded?.Invoke(this, winningTeam.Values.ToList(), losingTeam.Values.ToList());

            StopCoroutine(turnTimerCoroutine);

            teamA = new();
            teamB = new();
        }
    }
}