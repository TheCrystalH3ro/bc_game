using System.Collections;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers.Server
{
    public class PartyServerController : NetworkBehaviour
    {
         private static PartyServerController  _instance;

        public static PartyServerController Singleton
        { 
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PartyServerController>();
                }

                if(_instance == null)
                {
                    if(! InstanceFinder.NetworkManager.IsServerStarted) return null;

                    GameObject controller = new();

                    _instance = controller.AddComponent<PartyServerController>();

                    InstanceFinder.ServerManager.Spawn(controller);
                }
                
                return _instance;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void InvitePlayer(PlayerCharacter invitedPlayer)
        {
            PartyModule.Singleton.InvitePlayer(invitedPlayer.GetId(), OnInvitePlayerSuccess, OnInvitePlayerFail);
        }

        private void OnInvitePlayerSuccess(NetworkConnection invitedClient, PlayerCharacter invitedFrom)
        {
            NotifyPlayersInvite(invitedClient, invitedFrom);
        }

        private void OnInvitePlayerFail(NetworkConnection invitedClient, PlayerCharacter invitedFrom, string errorMessage)
        {

        }

        [TargetRpc]
        private void NotifyPlayersInvite(NetworkConnection invitedClient, PlayerCharacter invitedFrom)
        {

        }

        [ServerRpc(RequireOwnership = false)]
        public void AcceptInvite()
        {
            PartyModule.Singleton.AcceptInvite(OnPartyAcceptSuccess, OnPartyAcceptFail);
        }
        
        private void OnPartyAcceptSuccess(NetworkConnection invitedClient)
        {

        }

        private void OnPartyAcceptFail(NetworkConnection invitedClient, string errorMessage)
        {

        }

        [ServerRpc(RequireOwnership = false)]
        public void DeclineInvite()
        {
            PartyModule.Singleton.DeclineInvite(OnPartyDeclineSuccess, OnPartyDeclineFail);
        }

        private void OnPartyDeclineSuccess(NetworkConnection invitedClient, PlayerCharacter invitedBy, Party party)
        {
            //Notify leader
            if(party == null || party.IsLeader(invitedBy.GetId()))
            {
            }
        }

        private void OnPartyDeclineFail(NetworkConnection invitedClient, string errorMessage)
        {

        }

        [ServerRpc(RequireOwnership = false)]
        public void LeaveParty()
        {
            PartyModule.Singleton.LeaveParty(OnPartyLeaveSuccess, OnPartyLeaveFail);
        }

        private void OnPartyLeaveSuccess(NetworkConnection invitedClient, Party party)
        {

        }

        private void OnPartyLeaveFail(NetworkConnection invitedClient, string errorMessage)
        {

        }

        [ServerRpc(RequireOwnership = false)]
        public void KickPlayer(PlayerCharacter kickedPlayerCharacter)
        {
            PartyModule.Singleton.KickPlayer(kickedPlayerCharacter, OnPartyKickSuccess, OnPartyKickFail);
        }

        private void OnPartyKickSuccess(NetworkConnection invitedClient, PlayerCharacter kickedPlayer)
        {

        }

        private void OnPartyKickFail(NetworkConnection invitedClient, string errorMessage)
        {

        }
    }
}
