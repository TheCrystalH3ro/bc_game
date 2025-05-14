using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class MessageBox : MonoBehaviour
    {
        [SerializeField] private TMP_Text message;

        public void DisplayMessage(string text) {

            message.text = text;
            gameObject.SetActive(true);
        }

        public void CloseWindow()
        {
            gameObject.SetActive(false);
        }
    }
}
