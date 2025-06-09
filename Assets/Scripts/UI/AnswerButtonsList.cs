using System.Collections.Generic;
using UnityEngine;
    
namespace Assets.Scripts.UI
{
    public class AnswerButtonsList : MonoBehaviour
    {
        [SerializeField] private List<AnswerButton> buttons;

        public void SetAnswers(Dictionary<uint, string> answers)
        {
            int index = 0;
            foreach (KeyValuePair<uint, string> answer in answers)
            {
                buttons[index].SetAnswer(answer.Key, index, answer.Value);
                index++;
            }
        }

        public void SetButtonsEnabled(bool enabled)
        {
            foreach (AnswerButton button in buttons)
            {
                button.SetEnabled(enabled);
            }
        }

        public void SetAnswerResult(int answerId, bool isCorrect)
        {
            buttons[answerId].ShowResult(isCorrect);
        }

        public void ClearAnswerResult(int answerId)
        {
            SetButtonsEnabled(true);
            buttons[answerId].ClearResult();
        }
    }
}