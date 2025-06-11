using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public abstract class AttackModule : MonoBehaviour
    {
        [SerializeField] private int BASE_DAMAGE = 10;
        [SerializeField][Range(0, 1)] private float perfectTimeThreshold = 0.8f;
        [SerializeField][Range(0, 1)] private float goodTimeThreshold = 0.65f;
        [SerializeField][Range(0, 1)] private float averageTimeThreshold = 0.4f;

        private List<StatusEffect> buffs = new();

        public float GetTimeThreshold(ReactionTime reactionTime)
        {
            return reactionTime switch
            {
                ReactionTime.PERFECT => perfectTimeThreshold,
                ReactionTime.GOOD => goodTimeThreshold,
                _ => averageTimeThreshold,
            };
        }

        public ReactionTime GetReactionTime(FlashCard flashCard, float time)
        {
            float questionTime = flashCard.GetTime();

            float perfectTime = GetTimeThreshold(ReactionTime.PERFECT) * questionTime;
            float goodTime = GetTimeThreshold(ReactionTime.GOOD) * questionTime;
            float averageTime = GetTimeThreshold(ReactionTime.AVG) * questionTime;

            if (time >= perfectTime)
                return ReactionTime.PERFECT;

            if (time >= goodTime)
                return ReactionTime.GOOD;

            if (time >= averageTime)
                return ReactionTime.AVG;

            return ReactionTime.POOR;
        }

        public int GetBaseDamage()
        {
            return BASE_DAMAGE;
        }

        public int DoAttack(BaseCharacterController target, int damage)
        {
            HealthModule targetHealth = target.GetComponent<HealthModule>();
            int targetHp = targetHealth.TakeHP(damage);

            return targetHp;
        }

        public virtual int GetDamage(FlashCard flashCard, float remainingTime)
        {
            int damage = BASE_DAMAGE;

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
                return damage * 2;

            if (remainingTime >= goodTime)
                return Mathf.CeilToInt(damage * 1.5f);

            if (remainingTime >= averageTime)
                return Mathf.FloorToInt(damage * 1.2f);

            return damage;
        }

        public virtual float GetDefense(FlashCard flashCard, float remainingTime)
        {
            float defense = 1;

            if (HasBuff(StatusEffectType.DEFENSE))
            {
                StatusEffect buff = GetBuff(StatusEffectType.DEFENSE);
                UseBuff(StatusEffectType.DEFENSE);

                defense *= buff.GetValue();
            }

            float questionTime = flashCard.GetTime();

            float perfectTime = GetTimeThreshold(ReactionTime.PERFECT) * questionTime;
            float goodTime = GetTimeThreshold(ReactionTime.GOOD) * questionTime;
            float averageTime = GetTimeThreshold(ReactionTime.AVG) * questionTime;

            if (remainingTime >= perfectTime)
                return 0.2f / defense;

            if (remainingTime >= goodTime)
                return 0.35f / defense;

            if (remainingTime >= averageTime)
                return 0.5f / defense;

            return 0.75f / defense;
        }

        public StatusEffect GetBuff(StatusEffectType type)
        {
            return buffs.Where(b => b.GetEffectType() == type)
                .OrderByDescending(b => b.GetValue())
                .FirstOrDefault();
        }

        public bool HasBuff(StatusEffectType type)
        {
            return GetBuff(type) != null;
        }

        public bool HasBuff(StatusEffect buff)
        {
            return buffs.FirstOrDefault(b => b.Equals(buff)) != null;
        }

        public int GetBuffIndex(StatusEffect other)
        {
            StatusEffect buff = buffs.FirstOrDefault(b => b.Equals(other));
            return buffs.IndexOf(buff);
        }

        public void AddBuff(StatusEffectType buffType, float value, uint duration)
        {
            StatusEffect buff = new(buffType, value, duration);

            if (HasBuff(buff))
            {
                int index = GetBuffIndex(buff);
                buffs[index].SetDuration(Math.Max(buffs[index].GetDuration(), duration));
                return;
            }

            buffs.Add(buff);
        }

        public void UseBuff(StatusEffectType type)
        {
            if (!HasBuff(type))
                return;

            foreach (StatusEffect buff in buffs)
            {
                if (buff.GetEffectType() != type)
                    continue;

                int index = buffs.IndexOf(buff);

                buff.DecreaseDuration();

                if (buff.GetDuration() == 0)
                {
                    buffs.Remove(buff);
                    return;
                }

                buffs[index] = buff;
            }
        }

        public void Stun()
        {
            AddBuff(StatusEffectType.STUN, 1, 1);
        }

        public bool IsStunned()
        {
            return HasBuff(StatusEffectType.STUN);
        }

        public void UseStun()
        {
            UseBuff(StatusEffectType.STUN);
        }

        public abstract int BeforeDamage(int damage, BaseCharacterController target, Answer answer, Answer targetAnswer);
        public abstract int BeforeFinalDamage(int finalDamage, int originalDamage, float defense, BaseCharacterController target, Answer answer, Answer targetAnswer);
        public abstract void AfterDamage(BaseCharacterController target, Answer answer, Answer targetAnswer);

        public abstract float BeforeDefense(float defense, BaseCharacterController attacker, Answer answer, Answer attackerAnswer);
        public abstract void AfterDefense(BaseCharacterController attacker, Answer answer, Answer attackerAnswer);
    }
}