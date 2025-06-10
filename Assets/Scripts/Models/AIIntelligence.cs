using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class AIIntelligence
    {
        private int knowledge;
        private ReactionThreshold reactionThreshold;
        private int learningRate;

        public AIIntelligence(int knowledge, ReactionThreshold reactionThreshold, int learningRate)
        {
            this.knowledge = knowledge;
            this.reactionThreshold = reactionThreshold;
            this.learningRate = learningRate;
        }

        public int GetKnowledge()
        {
            return knowledge;
        }

        public ReactionThreshold GetReactionThreshold()
        {
            return reactionThreshold;
        }

        public int GetLearningRate()
        {
            return learningRate;
        }

        public void DecreaseReactionThreshold(ReactionTime reactionTime, int threshold)
        {
            reactionThreshold.DecreaseReactionThreshold(reactionTime, threshold);
        }

        public void IncreaseKnowledge(int amount)
        {
            knowledge += amount;
        }
    }
}