using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using Assets.Scripts.UI;
using Assets.Scripts.UI.Controllers;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class GameController : NetworkBehaviour
    {
        public static GameController Singleton { get; private set; }

        void OnEnable()
        {
            PlayerController.OnEscapePressed += TogglePauseMenu;
        }

        void OnDisable()
        {
            PlayerController.OnEscapePressed -= TogglePauseMenu;
        }

        public override void OnStartClient()
        {
            if (!base.IsOwner) return;

            Singleton = this;

            string jwtToken = PlayerPrefs.GetString("authToken");
            uint characterId = (uint) PlayerPrefs.GetInt("CharacterId");

            GameServerController.Singleton.RequestToJoinServerRpc(jwtToken, characterId);
        }

        private void TogglePauseMenu()
        {
            HUDController.Singleton.PauseMenu.TogglePauseMenu();
        }

        public void InspectPlayer(int playerId, PlayerCharacter character, Sprite avatar)
        {
            HUDController.Singleton.PlayerCard.Init(playerId, character, avatar);
        }

        public void UpdatePartyUI(List<PlayerCharacter> playerCharacters, uint leaderId)
        {
        
        }
    }
}
