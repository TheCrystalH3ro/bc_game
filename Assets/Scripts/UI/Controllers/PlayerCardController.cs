using System.Collections;
using Assets.Scripts.Controllers;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using FishNet;
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
        private PlayerCharacter playerCharacter;

        public void Init(int playerId, PlayerCharacter playerCharacter, Sprite avatar)
        {
            this.playerId = playerId;
            this.playerCharacter = playerCharacter;

            playerName.SetText(playerCharacter.GetName());
            playerLvl.SetText("Level " + playerCharacter.GetLevel());
            playerAvatar.overrideSprite = avatar;

            GetComponent<CanvasGroup>().alpha = 1;
        }

        public void InviteToParty()
        {
            IParty party = PlayerController.Singleton.GetParty();
            int myId = InstanceFinder.ClientManager.Connection.ClientId;

            if (party != null && !party.IsLeader(myId))
            {
                HUDController.Singleton.ShowMessage("Only party leader can do that!");
                return;
            }

            if (party != null && party.IsMember(playerId))
            {
                PartyController.Singleton.ShowKickPlayerPrompt(playerCharacter);
                return;
            }
                
            PartyController.Singleton.ShowInvitePlayerPrompt(playerCharacter);
        }

        public void CloseCard()
        {
            GetComponent<CanvasGroup>().alpha = 0;
        }
    }
}
