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

        public void SetButtonsActive(bool active)
        {
            actionPanel.SetButtonsActive(active);
        }
    }
}