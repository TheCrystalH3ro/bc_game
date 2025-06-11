using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Models;
using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class FloatTextModule : MonoBehaviour
    {
        [SerializeField] private FloatText floatTextPrefab;
        [SerializeField] private float delay = 0.15f;

        private Queue<FloatTextItem> queue = new();
        private bool isProcessing = false;

        public void DisplayFloatText(string text, Color color)
        {
            FloatTextItem item = new()
            {
                Text = text,
                Color = color
            };

            EnqueueItem(item);
        }

        private void EnqueueItem(FloatTextItem item)
        {
            queue.Enqueue(item);

            if (!isProcessing)
                StartCoroutine(ProcessQueue());
        }

        private IEnumerator ProcessQueue()
        {
            isProcessing = true;

            while (queue.Count > 0)
            {
                FloatTextItem item = queue.Dequeue();

                FloatText floatText = Instantiate(floatTextPrefab, gameObject.transform);

                floatText.SetText(item.Text);
                floatText.SetColor(item.Color);

                yield return new WaitForSeconds(delay);
            }

            isProcessing = false;
        }
    }
}