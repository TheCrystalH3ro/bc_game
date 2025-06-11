using System.Collections.Generic;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Modules.Attack
{
    public class KnightAttackModule : AttackModule
    {
        public override int BeforeDamage(int damage, BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            if (!answer.IsCorrect)
                return Mathf.FloorToInt(GetBaseDamage() * 0.5f);

            return damage;
        }

        public override int BeforeFinalDamage(int finalDamage, int originalDamage, float defense, BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            return finalDamage;
        }

        public override void AfterDamage(BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            if (answer.RemainingTime > targetAnswer.RemainingTime)
                BuffTeammateDamage();
        }

        private void BuffTeammateDamage()
        {
            BaseCharacterController character = gameObject.GetComponent<BaseCharacterController>();
            CombatModule instance = CombatServerController.Singleton.GetInstance(character);

            if (instance == null)
                return;

            Dictionary<uint, BaseCharacterController> team = instance.GetTeam(character);

            foreach (BaseCharacterController teammate in team.Values)
                teammate.AddBuff(StatusEffectType.DAMAGE, 1.25f, 1);
        }

        public override float BeforeDefense(float defense, BaseCharacterController attacker, Answer answer, Answer attackerAnswer)
        {
            return defense;
        }

        public override void AfterDefense(BaseCharacterController attacker, Answer answer, Answer attackerAnswer)
        {
            if (!answer.IsCorrect)
                return;

            AddBuff(StatusEffectType.DEFENSE, 1.5f, 1);

            if (answer.RemainingTime > attackerAnswer.RemainingTime)
                attacker.Stun();
        }
    }
}