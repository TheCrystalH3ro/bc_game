using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class ActionPanel : MonoBehaviour
    {
        [SerializeField] private ActionButtons actions;
        [SerializeField] private AnswerButtonsList answerButtons;

        private GameObject activePanel;

        void OnEnable()
        {
            OpenButtons();
        }

        public void OpenButtons()
        {
            if (activePanel != null)
                activePanel.SetActive(false);

            actions.gameObject.SetActive(true);
            activePanel = actions.gameObject;
        }

        public void SetButtonsActive(bool active)
        {
            actions.SetEnabled(active);
        }

        public void SetAnswers(Dictionary<uint, string> answers)
        {
            if (activePanel != null)
                activePanel.SetActive(false);

            activePanel = answerButtons.gameObject;
            answerButtons.gameObject.SetActive(true);

            answerButtons.SetAnswers(answers);
        }
    }
}