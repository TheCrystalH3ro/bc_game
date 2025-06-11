namespace Assets.Scripts.Models
{
    public class Answer
    {
        public FlashCard FlashCard { get; private set; }
        public bool IsCorrect { get; private set; }
        public float RemainingTime { get; private set; }

        public Answer(FlashCard flashCard, bool correct, float remainingTime)
        {
            FlashCard = flashCard;
            IsCorrect = correct;
            RemainingTime = remainingTime;
        }
    }
}