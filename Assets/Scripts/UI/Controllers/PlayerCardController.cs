using System.Collections;
using Assets.Scripts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Controllers
{
    public class PlayerCardController : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private TMP_Text playerLvl;
        [SerializeField] private Image playerAvatar;

        private int playerId;

        public void Init(int playerId, PlayerCharacter playerCharacter, Sprite avatar) {
            this.playerId = playerId;

            playerName.SetText(playerCharacter.GetName());
            playerLvl.SetText("Level " + playerCharacter.GetLevel());
            playerAvatar.overrideSprite = avatar;

            GetComponent<CanvasGroup>().alpha = 1;
        }

        public void CloseCard() {
            GetComponent<CanvasGroup>().alpha = 0;
        }
    }
}
