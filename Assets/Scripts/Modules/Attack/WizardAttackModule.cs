using Assets.Scripts.Controllers;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Modules.Attack
{
    public class WizardAttackModule : AttackModule
    {
        private int answerStreak = 0;

        public override int BeforeDamage(int damage, BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            if (answerStreak <= 0)
            {
                return damage;
            }

            return Mathf.RoundToInt(damage * (1 + answerStreak * 0.25f));
        }

        public override int BeforeFinalDamage(int finalDamage, int originalDamage, float defense, BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            return finalDamage;
        }

        public override void AfterDamage(BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            if (answer.IsCorrect)
            {
                answerStreak++;
                return;
            }

            if (answerStreak > 0)
            {
                answerStreak = 0;
                return;
            }

            BaseCharacterController character = gameObject.GetComponent<BaseCharacterController>();
            character.DoAttack(character, GetBaseDamage());
        }

        public override float BeforeDefense(float defense, BaseCharacterController attacker, Answer answer, Answer attackerAnswer)
        {
            return defense;
        }

        public override void AfterDefense(BaseCharacterController attacker, Answer answer, Answer attackerAnswer)
        {
            BaseCharacterController character = gameObject.GetComponent<BaseCharacterController>();

            if (!answer.IsCorrect && !attackerAnswer.IsCorrect)
            {
                character.DoAttack(character, GetBaseDamage());
                character.DoAttack(attacker, GetBaseDamage());
                return;
            }

            if (!answer.IsCorrect)
            {
                character.DoAttack(character, GetBaseDamage());
                return;
            }

            if (!attackerAnswer.IsCorrect || (answer.RemainingTime > attackerAnswer.RemainingTime))
            {
                character.DoAttack(attacker, GetBaseDamage());
                return;
            }
        }
    }
}