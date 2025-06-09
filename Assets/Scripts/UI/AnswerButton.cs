using Assets.Scripts.Controllers;
using UnityEngine;
    
namespace Assets.Scripts.UI
{
    public class AnswerButton : MonoBehaviour
    {
        private uint id;
        [SerializeField] private TMPro.TMP_Text answer;

        public void SetAnswer(uint id, string text)
        {
            Debug.Log("Setting answer [" + id + "] to : " + text);
            this.id = id;
            answer.text = text;
        }

        public void SubmitAnswer()
        {
            CombatController.Singleton.AnswerQuestion(id);
        }
    }
}