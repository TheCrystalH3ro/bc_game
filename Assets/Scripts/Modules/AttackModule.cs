using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class AttackModule : MonoBehaviour
    {
        [SerializeField] private int BASE_DAMAGE = 10;
        [SerializeField][Range(0, 1)] private float perfectTimeThreshold = 0.8f;
        [SerializeField][Range(0, 1)] private float goodTimeThreshold = 0.65f;
        [SerializeField][Range(0, 1)] private float averageTimeThreshold = 0.4f;

        public float GetTimeThreshold(ReactionTime reactionTime)
        {
            return reactionTime switch
            {
                ReactionTime.PERFECT => perfectTimeThreshold,
                ReactionTime.GOOD => goodTimeThreshold,
                _ => averageTimeThreshold,
            };
        }

        public virtual int GetDamage(FlashCard flashCard, float remainingTime)
        {
            float questionTime = flashCard.GetTime();

            float perfectTime = GetTimeThreshold(ReactionTime.PERFECT) * questionTime;
            float goodTime = GetTimeThreshold(ReactionTime.GOOD) * questionTime;
            float averageTime = GetTimeThreshold(ReactionTime.AVG) * questionTime;

            if (remainingTime >= perfectTime)
                return BASE_DAMAGE * 2;

            if (remainingTime >= goodTime)
                return Mathf.CeilToInt(BASE_DAMAGE * 1.5f);

            if (remainingTime >= averageTime)
                return Mathf.FloorToInt(BASE_DAMAGE * 1.2f);

            return BASE_DAMAGE;
        }

        public virtual float GetDefense(FlashCard flashCard, float remainingTime)
        {
            float questionTime = flashCard.GetTime();

            float perfectTime = GetTimeThreshold(ReactionTime.PERFECT) * questionTime;
            float goodTime = GetTimeThreshold(ReactionTime.GOOD) * questionTime;
            float averageTime = GetTimeThreshold(ReactionTime.AVG) * questionTime;

            if (remainingTime >= perfectTime)
                return 0.2f;

            if (remainingTime >= goodTime)
                return 0.35f;

            if (remainingTime >= averageTime)
                return 0.5f;

            return 0.75f;
        }
    }
}