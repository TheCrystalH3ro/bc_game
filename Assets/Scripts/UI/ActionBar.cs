using System.Collections.Generic;
using Assets.Scripts.Controllers;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class ActionBar : MonoBehaviour
    {
        [SerializeField] private ActionLog actionLog;
        [SerializeField] private ActionPanel actionPanel;

        public void SetCharacterTurn(BaseCharacterController character)
        {
            actionLog.SetCharacterTurn(character);
        }

        public void SetCharacterAttack(BaseCharacterController character, BaseCharacterController target)
        {
            actionLog.SetCharacterAttack(character, target);
        }

        public void SetCharacterDeath(BaseCharacterController character)
        {
            actionLog.SetCharacterDeath(character);
        }

        public void OpenButtons()
        {
            actionPanel.OpenButtons();
        }

        public void SetButtonsActive(bool active)
        {
            actionPanel.SetButtonsActive(active);
        }

        public void SetQuestion(string question, Dictionary<uint, string> answers)
        {
            actionLog.SetQuestion(question);
            actionPanel.SetAnswers(answers);
        }

        public void AnswerSubmitted()
        {
            actionPanel.AnswerSubmitted();
        }

        public void SetAnswerResult(int answerId, bool isCorrect)
        {
            actionPanel.SetAnswerResult(answerId, isCorrect);
        }

        public void ClearAnswerResult(int answerId)
        {
            actionPanel.ClearAnswerResult(answerId);
        }

        public void TurnPassed(BaseCharacterController character)
        {
            actionLog.SetTurnPassed(character);
        }
    }
}