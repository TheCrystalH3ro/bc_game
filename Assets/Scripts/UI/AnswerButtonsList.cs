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
                buttons[index].SetAnswer(answer.Key, answer.Value);
                index++;
            }
        }
    }
}