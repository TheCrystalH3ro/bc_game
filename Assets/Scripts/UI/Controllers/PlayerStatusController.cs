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

        public void Init(PlayerCharacter character, Sprite playerAvatar, int maxHealth, float maxExp, bool inSameZone = true)
        {
            avatar.overrideSprite = playerAvatar;
            playerName.SetText(character.GetName());
            healthIndicator.SetMaxValue(maxHealth);
            expIndicator.SetMaxValue((int)maxExp);
            levelText.SetText(character.GetLevel().ToString());
            gameObject.GetComponent<CanvasGroup>().alpha = inSameZone ? 1f : 0.5f;
        }

        public void UpdateHealth(int health)
        {
            healthIndicator.SetValue(health);
        }

        public void UpdateExp(float exp)
        {
            expIndicator.SetValue((int)exp);
        }

        public void UpdateLvl(int lvl)
        {
            levelText.SetText(lvl.ToString());
        }

        public void SetTransparency(float value)
        {
            gameObject.GetComponent<CanvasGroup>().alpha = value;
        }
    }
}
