using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class FloatText : MonoBehaviour
    {
        private TMP_Text message;

        void Awake()
        {
            message = GetComponent<TMP_Text>();
        }

        public void SetText(string text)
        {
            message.text = text;
        }

        public void SetColor(Color color)
        {
            message.color = color;
        }
    }
}