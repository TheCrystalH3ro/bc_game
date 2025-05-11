using System.Collections;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class PartyController : MonoBehaviour
    {
        public static PartyController Singleton { get; private set; }

        void OnEnable()
        {
            Singleton = FindFirstObjectByType<PartyController>();
        }

        public void InvitePlayerButton(PlayerCharacter invitedPlayer)
        {
            PartyServerController.Singleton.InvitePlayer(invitedPlayer);
        }

        public void AcceptInviteButton()
        {
            PartyServerController.Singleton.AcceptInvite();
        }

        public void DeclineInviteButton()
        {
            PartyServerController.Singleton.DeclineInvite();
        }

        public void LeavePartyButton()
        {
            PartyServerController.Singleton.LeaveParty();
        }

        public void KickPlayerButton(PlayerCharacter kickedPlayerCharacter)
        {
            PartyServerController.Singleton.KickPlayer(kickedPlayerCharacter);
        }
    }
}
