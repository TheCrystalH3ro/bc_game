using Assets.Scripts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Controllers
{
    public class CharacterStatusController : MonoBehaviour
    {
        [SerializeField] private Image avatar;
        [SerializeField] private TMP_Text characterName;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Indicator healthIndicator;

        public void Init(CharacterData character)
        {
            avatar.overrideSprite = character.GetSprite();

            characterName.SetText(character.GetName());
            levelText.SetText(character.GetLevel().ToString());

            healthIndicator.SetMaxValue(character.GetMaxHealth());
            healthIndicator.SetValue(character.GetHealth());
        }

        public void UpdateHealth(int health)
        {
            healthIndicator.SetValue(health);
        }
    }
}