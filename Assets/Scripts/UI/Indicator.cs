using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class Indicator : MonoBehaviour
    {
        [SerializeField] private RectTransform barRect;
        [SerializeField] private RectMask2D barMask;
        [SerializeField] TMP_Text barText;

        private int value = 0;
        private int maxValue = 1;

        public void SetValue(int value)
        {
            this.value = value;
            UpdateBar();
        }

        public void SetMaxValue(int maxValue)
        {
            this.maxValue = maxValue;
            UpdateBar();
        }

        private void UpdateBar()
        {
            barText.SetText(value + " / " + maxValue);
            float barWidth = value * barRect.rect.width / maxValue;
            var padding = barMask.padding;
            padding.z = barRect.rect.width - barWidth;
            barMask.padding = padding;
        }
    }
}
