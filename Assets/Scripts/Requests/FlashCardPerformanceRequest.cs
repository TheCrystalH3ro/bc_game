using System;
using UnityEngine;

namespace Assets.Scripts.Requests
{
    [Serializable]
    public class FlashCardPerformanceRequest : BaseRequest
    {
        public uint flashCardId;
        public bool isCorrect;
        public float time;

        public FlashCardPerformanceRequest(uint flashCardId, bool isCorrect, float time)
        {
            this.flashCardId = flashCardId;
            this.isCorrect = isCorrect;
            this.time = time;
        }
    }
}