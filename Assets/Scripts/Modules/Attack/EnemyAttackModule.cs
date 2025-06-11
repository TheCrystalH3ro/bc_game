using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets.Scripts.Controllers;
using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Modules.Attack
{
    public class EnemyAttackModule : AttackModule
    {
        private AIIntelligence intelligence;
        private Dictionary<FlashCard, AIIntelligence> questionKnowledge = new();
        [SerializeField] private IntelligenceLevel intelligenceLevel;

        void Start()
        {
            InitializeIntelligence();
        }

        private void InitializeIntelligence()
        {
            int knowledge;
            ReactionThreshold reactionThreshold;
            int learningRate;

            switch (intelligenceLevel)
            {
                case IntelligenceLevel.VERYHIGH:
                    knowledge = 60;
                    reactionThreshold = new
                    (
                        5,
                        25,
                        40,
                        60
                    );
                    learningRate = 10;
                    break;
                case IntelligenceLevel.HIGH:
                    knowledge = 40;
                    reactionThreshold = new
                    (
                        10,
                        35,
                        55,
                        70
                    );
                    learningRate = 8;
                    break;
                case IntelligenceLevel.MEDIUM:
                    knowledge = 30;
                    reactionThreshold = new
                    (
                        15,
                        40,
                        65,
                        80
                    );
                    learningRate = 5;
                    break;
                case IntelligenceLevel.LOW:
                    knowledge = 20;
                    reactionThreshold = new
                    (
                        20,
                        50,
                        75,
                        90
                    );
                    learningRate = 3;
                    break;
                default:
                    knowledge = 0;
                    reactionThreshold = new
                    (
                        30,
                        60,
                        80,
                        100
                    );
                    learningRate = 1;
                    break;
            }

            intelligence = new(knowledge, reactionThreshold, learningRate);
        }

        private AIIntelligence CreateQuestionIntelligence()
        {
            var learningRate = intelligenceLevel switch
            {
                IntelligenceLevel.VERYHIGH => 15,
                IntelligenceLevel.HIGH => 10,
                IntelligenceLevel.MEDIUM => 7,
                IntelligenceLevel.LOW => 5,
                _ => 2,
            };
            return new(0, intelligence.GetReactionThreshold(), learningRate);
        }

        public float GetThinkingTime(FlashCard flashCard)
        {
            ReactionThreshold reactionSpeed = intelligence.GetReactionThreshold();

            if (questionKnowledge.ContainsKey(flashCard))
            {
                AIIntelligence questionIntelligence = questionKnowledge[flashCard];
                reactionSpeed = reactionSpeed.CombineThresholds(questionIntelligence.GetReactionThreshold());
            }

            int speedIndex = Random.Range(0, 101);
            ReactionTime reactionTime = reactionSpeed.GetReactionTime(speedIndex);

            return reactionTime switch
            {
                ReactionTime.PERFECT => Random.Range(0, flashCard.GetTime() * 0.2f),
                ReactionTime.GOOD => Random.Range(flashCard.GetTime() * 0.21f, flashCard.GetTime() * 0.35f),
                ReactionTime.AVG => Random.Range(flashCard.GetTime() * 0.36f, flashCard.GetTime() * 0.6f),
                ReactionTime.POOR => Random.Range(flashCard.GetTime() * 0.61f, flashCard.GetTime()),
                _ => flashCard.GetTime(),
            };
        }

        public uint GetQuestionAnswer(FlashCard flashCard)
        {
            float qKnowledge = 0f;

            if (questionKnowledge.ContainsKey(flashCard))
            {
                AIIntelligence questionIntelligence = questionKnowledge[flashCard];
                qKnowledge = questionIntelligence.GetKnowledge();
            }

            float effectiveKnowledge = qKnowledge + intelligence.GetKnowledge();

            float randomIndex = Random.Range(0f, 100f);
            bool correct = effectiveKnowledge >= randomIndex;

            if (correct)
                return flashCard.GetCorrectAnswer();

            List<uint> answers = flashCard.GetAnswers().Where(answer => !flashCard.IsCorrectAnswer(answer.Key)).Select(answer => answer.Key).ToList();
            int answerIndex = Random.Range(0, answers.Count);

            return answers.ElementAt(answerIndex);
        }

        public void Learn(FlashCard flashCard, bool isCorrect, float answeredTime)
        {
            AIIntelligence questionIntelligence = questionKnowledge.ContainsKey(flashCard) ? questionKnowledge[flashCard] : CreateQuestionIntelligence();

            int questionLearningRate = questionIntelligence.GetLearningRate();
            int questionKnowledgeIncrease = questionLearningRate;

            if (isCorrect)
            {
                intelligence.IncreaseKnowledge(intelligence.GetLearningRate());

                questionKnowledgeIncrease *= 3;

                ReactionTime reactionTime = GetReactionTime(flashCard, answeredTime);
                questionIntelligence.DecreaseReactionThreshold(reactionTime, Mathf.FloorToInt(questionLearningRate * 0.5f));
            }

            questionIntelligence.IncreaseKnowledge(questionKnowledgeIncrease);

            questionKnowledge[flashCard] = questionIntelligence;
        }

        public override int BeforeDamage(int damage, BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            return damage;
        }

        public override int BeforeFinalDamage(int finalDamage, int originalDamage, float defense, BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            return finalDamage;
        }

        public override void AfterDamage(BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
        }

        public override float BeforeDefense(float defense, BaseCharacterController attacker, Answer answer, Answer attackerAnswer)
        {
            return defense;
        }

        public override void AfterDefense(BaseCharacterController attacker, Answer answer, Answer attackerAnswer)
        {
        }
    }
}