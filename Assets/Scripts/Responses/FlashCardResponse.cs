using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Responses
{
    [Serializable]
    public class AnswerData
    {
        public uint id;
        public string text;
    }

    [Serializable]
    public class FlashCardResponse
    {
        public uint id;
        public string question;
        public AnswerData[] answers;
        public uint correctAnswer;
        public float time;

        public FlashCardResponse(uint id, string question, AnswerData[] answers, uint correctAnswer, float time)
        {
            this.id = id;
            this.question = question;
            this.answers = answers;
            this.correctAnswer = correctAnswer;
            this.time = time;
        }

        public static FlashCardResponse CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<FlashCardResponse>(jsonString);
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }
    }
}