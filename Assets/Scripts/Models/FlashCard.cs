using System.Collections.Generic;
using Assets.Scripts.Responses;

namespace Assets.Scripts.Models
{
    public class FlashCard
    {
        private uint id;
        private string question;
        private Dictionary<uint, string> answers;

        [System.NonSerialized]
        private readonly uint correctAnswer;
        private readonly float time;
        private readonly uint level = 0;

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

        public FlashCard(FlashCardResponse response)
        {
            this.id = response.id;
            this.question = response.question;
            this.answers = new();
            this.correctAnswer = response.correctAnswer;
            this.time = response.time;
            this.level = response.level;

            foreach (AnswerData answer in response.answers)
                answers.Add(answer.id, answer.text);
        }

        public bool Equals(FlashCard other)
        {
            return question.Equals(other.GetQuestion());
        }

        public uint GetId()
        {
            return id;
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

        public float GetDamageBuff()
        {
            if (level == 0)
                return 1;

            return 1 + 0.2f * (level - 1);
        }
    }
}