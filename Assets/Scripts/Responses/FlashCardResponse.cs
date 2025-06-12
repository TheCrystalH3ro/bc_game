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
    public class FlashCardResponse : BaseResponse
    {
        public uint id;
        public string question;
        public AnswerData[] answers;
        public uint correctAnswer;
        public float time;
        public uint level;

        public FlashCardResponse(uint id, string question, AnswerData[] answers, uint correctAnswer, float time, uint level)
        {
            this.id = id;
            this.question = question;
            this.answers = answers;
            this.correctAnswer = correctAnswer;
            this.time = time;
            this.level = level;
        }
    }
}