using Assets.Scripts.Controllers;
using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Modules.Attack
{
    public class RogueAttackModule : AttackModule
    {
        public override int GetDamage(FlashCard flashCard, float remainingTime)
        {
            int damage = GetBaseDamage();

            if (HasBuff(StatusEffectType.DAMAGE))
            {
                StatusEffect buff = GetBuff(StatusEffectType.DAMAGE);
                UseBuff(StatusEffectType.DAMAGE);

                damage = Mathf.FloorToInt(damage * buff.GetValue());
            }

            float questionTime = flashCard.GetTime();

            float perfectTime = GetTimeThreshold(ReactionTime.PERFECT) * questionTime;
            float goodTime = GetTimeThreshold(ReactionTime.GOOD) * questionTime;
            float averageTime = GetTimeThreshold(ReactionTime.AVG) * questionTime;

            if (remainingTime >= perfectTime)
                return Mathf.CeilToInt(damage * 2.5f);

            if (remainingTime >= goodTime)
                return Mathf.CeilToInt(damage * 2f);

            if (remainingTime >= averageTime)
                return Mathf.FloorToInt(damage * 0.85f);

            return Mathf.CeilToInt(damage * 0.5f);
        }

        public override int BeforeDamage(int damage, BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            return damage;
        }

        public override int BeforeFinalDamage(int finalDamage, int originalDamage, float defense, BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            ReactionTime reactionTime = GetReactionTime(answer.FlashCard, answer.RemainingTime);

            if (reactionTime == ReactionTime.PERFECT && answer.RemainingTime > targetAnswer.RemainingTime)
                return originalDamage;

            return finalDamage;
        }

        public override void AfterDamage(BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
        }

        public override float BeforeDefense(float defense, BaseCharacterController attacker, Answer answer, Answer attackerAnswer)
        {
            ReactionTime reactionTime = GetReactionTime(answer.FlashCard, answer.RemainingTime);

            if (answer.IsCorrect && reactionTime == ReactionTime.PERFECT && (answer.RemainingTime > attackerAnswer.RemainingTime))
                return 0f;

            return defense;
        }

        public override void AfterDefense(BaseCharacterController attacker, Answer answer, Answer attackerAnswer)
        {
        }
    }
}