using System.Collections;
using Assets.Scripts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Controllers
{
    public class PlayerStatusController : MonoBehaviour
    {
        [SerializeField] private Image avatar;
        [SerializeField] private TMP_Text playerName;

        [SerializeField] private Indicator healthIndicator;
        [SerializeField] private Indicator expIndicator;
        [SerializeField] private TMP_Text levelText;

        public void Init(PlayerCharacter character, Sprite playerAvatar, int maxHealth, int maxExp) {
            avatar.overrideSprite = playerAvatar;
            playerName.SetText(character.GetName());
            healthIndicator.SetMaxValue(maxHealth);
            expIndicator.SetMaxValue(maxExp);
            levelText.SetText(character.GetLevel().ToString());
            gameObject.GetComponent<CanvasGroup>().alpha = 1;
        }

        public void UpdateHealth(int health) {
            healthIndicator.SetValue(health);
        }

        public void UpdateExp(int exp) {
            expIndicator.SetValue(exp);
        }

        public void UpdateLvl(int lvl) {
            levelText.SetText(lvl.ToString());
        }
    }
}
