using UnityEngine;

namespace Assets.Scripts.UI
{
    public class ActionPanel : MonoBehaviour
    {
        [SerializeField] private ActionButtons buttons;

        private GameObject activePanel;

        void OnEnable()
        {
            OpenButtons();
        }

        public void OpenButtons()
        {
            if (activePanel != null)
                activePanel.SetActive(false);

            buttons.gameObject.SetActive(true);
            activePanel = buttons.gameObject;
        }

        public void SetButtonsActive(bool active)
        {
            buttons.SetEnabled(active);
        }
    }
}