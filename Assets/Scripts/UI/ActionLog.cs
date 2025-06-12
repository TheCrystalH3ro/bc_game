using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class ActionLog : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text logText;

        private void SetText(string text)
        {
            logText.text = text;
        }

        public void SetCharacterTurn(BaseCharacterController character)
        {
            SetText(character.ToString() + " is on turn.");
        }

        public void SetCharacterAttack(BaseCharacterController character, BaseCharacterController target)
        {
            SetText(character.ToString() + " attacked " + target.ToString());
        }

        public void SetCharacterDeath(BaseCharacterController character)
        {
            SetText(character.ToString() + " was defeated.");
        }

        public void SetQuestion(string question)
        {
            SetText("Question : " + question);
        }

        public void SetTurnPassed(BaseCharacterController character)
        {
            SetText(character.ToString() + " passed his turn.");
        }
    }
}