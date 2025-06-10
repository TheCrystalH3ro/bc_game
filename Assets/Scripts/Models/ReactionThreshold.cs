using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class ReactionThreshold
    {
        [Range(0, 100)]
        public int poor = 15;
        [Range(0, 100)]
        public int avg = 30;
        [Range(0, 100)]
        public int good = 50;
        [Range(0, 100)]
        public int perfect = 90;

        public ReactionThreshold(int poorThreshold, int avgThreshold, int goodThreshold, int perfectThreshold)
        {
            poor = Mathf.Clamp(poorThreshold, 0, 100);
            avg = Mathf.Clamp(avgThreshold, 0, 100);
            good = Mathf.Clamp(goodThreshold, 0, 100);
            perfect = Mathf.Clamp(perfectThreshold, 0, 100);
        }

        public ReactionThreshold CombineThresholds(ReactionThreshold threshold)
        {
            return new(
                (poor + threshold.poor) / 2,
                (avg + threshold.avg) / 2,
                (good + threshold.good) / 2,
                (perfect + threshold.perfect) / 2
            );
        }

        public ReactionTime GetReactionTime(int index)
        {
            if (index >= perfect)
                return ReactionTime.PERFECT;

            if (index >= good)
                return ReactionTime.GOOD;

            if (index >= avg)
                return ReactionTime.AVG;

            if (index >= poor)
                return ReactionTime.POOR;

            return ReactionTime.NONE;
        }

        public void DecreaseReactionThreshold(ReactionTime reactionTime, int amount)
        {
            switch (reactionTime)
            {
                case ReactionTime.PERFECT:
                    perfect -= amount;
                    break;
                case ReactionTime.GOOD:
                    good -= amount;
                    break;
                case ReactionTime.AVG:
                    avg -= amount;
                    break;
                case ReactionTime.POOR:
                    poor -= amount;
                    break;
            }
        }
    }
}