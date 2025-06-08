using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class ActionButtons : MonoBehaviour
    {
        [SerializeField] private Button attackButton;
        [SerializeField] private Button passButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button escapeButton;

        public void SetEnabled(bool enabled)
        {
            attackButton.interactable = enabled;
            passButton.interactable = enabled;
            inventoryButton.interactable = enabled;
            escapeButton.interactable = enabled;
        }
    }
}