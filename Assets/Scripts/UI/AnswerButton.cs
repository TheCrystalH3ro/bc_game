using Assets.Scripts.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class AnswerButton : MonoBehaviour
    {
        private uint id;
        private int index;
        [SerializeField] private TMPro.TMP_Text answer;

        private Button button;

        private Color defaultColor;
        private Color correctColor = new(0.5960751f, 0.8962264f, 0.6080804f);
        private Color wrongColor = new(0.9339623f, 0.4273318f, 0.4934707f);

        void Awake()
        {
            button = GetComponent<Button>();
            defaultColor = button.colors.disabledColor;
        }

        public void SetAnswer(uint id, int index, string text)
        {
            SetColor(defaultColor);
            this.id = id;
            this.index = index;
            answer.text = text;
            button.interactable = true;
        }

        public void SubmitAnswer()
        {
            CombatController.Singleton.AnswerQuestion(id, index);
        }

        public void ShowResult(bool isCorrect)
        {
            SetColor(isCorrect ? correctColor : wrongColor);
        }

        public void ClearResult()
        {
            SetColor(defaultColor);
        }

        private void SetColor(Color color)
        {
            ColorBlock colors = button.colors;
            colors.disabledColor = color;

            button.colors = colors;
        }

        public void SetEnabled(bool enabled)
        {
            button.interactable = enabled;
        }
    }
}