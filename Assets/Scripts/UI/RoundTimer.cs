using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class RoundTimer : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text timeDisplay;
        [SerializeField] private Image timeFill;

        private int maxTime = 0;

        public void SetMaxTime(int time)
        {
            maxTime = time;
            SetTime(time);
        }

        public void SetTime(int time)
        {
            timeDisplay.text = time.ToString();

            timeFill.fillAmount = (float) time / maxTime;
        }
    }
}