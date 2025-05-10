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

        private PauseMenu pauseMenu;
        [SerializeField] private GameObject playerCardPrefab;

        void OnEnable() {
            // pauseMenu = GameObject.FindWithTag("PauseMenu").GetComponent<PauseMenu>();
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
            pauseMenu.TogglePauseMenu();
        }

        public void InspectPlayer(int playerId, PlayerCharacter character, Sprite avatar)
        {
            GameObject playerCard = GameObject.FindGameObjectWithTag("PlayerCard");

            if(playerCard == null)
            {
                GameObject canvas = GameObject.FindGameObjectWithTag("MainCanvas");
                playerCard = Instantiate(playerCardPrefab, canvas.transform);
            }
        
            playerCard.GetComponent<PlayerCardController>().Init(playerId, character, avatar);
        }

        public void UpdatePartyUI(List<PlayerCharacter> playerCharacters, uint leaderId)
        {
        
        }
    }
}
