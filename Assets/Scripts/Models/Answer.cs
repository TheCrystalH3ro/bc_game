namespace Assets.Scripts.Models
{
    public class Answer
    {
        public bool IsCorrect { get; private set; }
        public float RemainingTime { get; private set; }

        public Answer(bool correct, float remainingTime)
        {
            IsCorrect = correct;
            RemainingTime = remainingTime;
        }
    }
}