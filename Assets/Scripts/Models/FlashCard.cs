using System.Collections.Generic;

namespace Assets.Scripts.Models
{
    public class FlashCard
    {
        private string question;
        private Dictionary<uint, string> answers;
        
        [System.NonSerialized]
        private readonly uint correctAnswer;
        private float time;

        public FlashCard()
        {
            this.question = "";
            this.answers = new();
            this.correctAnswer = 0;
            this.time = 0;
        }

        public FlashCard(string question, Dictionary<uint, string> answers, uint correctAnswer, float time)
        {
            this.question = question;
            this.answers = answers;
            this.correctAnswer = correctAnswer;
            this.time = time;
        }

        public bool Equals(FlashCard other)
        {
            return question.Equals(other.GetQuestion());
        }

        public string GetQuestion()
        {
            return question;
        }

        public Dictionary<uint, string> GetAnswers()
        {
            return answers;
        }

        public uint GetCorrectAnswer()
        {
            return correctAnswer;
        }

        public bool IsCorrectAnswer(uint answer)
        {
            return correctAnswer == answer;
        }

        public float GetTime()
        {
            return time;
        }
    }
}