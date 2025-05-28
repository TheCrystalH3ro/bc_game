using System.Linq;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using Assets.Scripts.UI.Controllers;
using Assets.Scripts.Util;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using GameKit.Dependencies.Utilities.Types;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Controllers
{
    public class GameController : MonoBehaviour
    {
        public static GameController Singleton { get; private set; }

        [SerializeField] private Sprite knightSprite;
        [SerializeField] private Sprite wizardSprite;
        [SerializeField] private Sprite rogueSprite;

        private void RegisterEvents()
        {
            InstanceFinder.SceneManager.OnQueueStart += LoadQueueStarted;
            InstanceFinder.SceneManager.OnQueueEnd += LoadQueueEnded;
            PlayerController.OnEscapePressed += TogglePauseMenu;
        }

        void OnDisable()
        {
            if (InstanceFinder.NetworkManager == null || InstanceFinder.NetworkManager.IsServerStarted)
                return;

            InstanceFinder.SceneManager.OnQueueStart -= LoadQueueStarted;
            InstanceFinder.SceneManager.OnQueueEnd -= LoadQueueEnded;
            PlayerController.OnEscapePressed -= TogglePauseMenu;
        }

        void OnDestroy()
        {
            DestroyObjects();
        }

        void Awake()
        {
            Singleton = this;
        }

        public void Initialize()
        {
            RegisterEvents();

            KeepObjects();

            string jwtToken = PlayerPrefs.GetString("authToken");
            uint characterId = (uint) PlayerPrefs.GetInt("CharacterId");

            GameServerController.Singleton.RequestToJoinServer(jwtToken, characterId);
        }

        private void KeepObjects()
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(GameServerController.Singleton.gameObject);
        }

        private void DestroyObjects()
        {
            Destroy(gameObject);

            if (GameServerController.Singleton != null && GameServerController.Singleton.gameObject != null)
                Destroy(GameServerController.Singleton.gameObject);
        }

        public Sprite GetCharacterSprite(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Wizard => wizardSprite,
                PlayerClass.Rogue => rogueSprite,
                _ => knightSprite,
            };
        }

        private void LoadQueueStarted()
        {
            HUDController.Singleton.ShowLoadingScreen();
        }

        private void LoadQueueEnded()
        {
            HUDController.Singleton.HideLoadingScreen();
        }

        private void TogglePauseMenu()
        {
            HUDController.Singleton.PauseMenu.TogglePauseMenu();
        }

        public void InspectPlayer(int playerId, PlayerCharacter character, Sprite avatar)
        {
            Debug.Log(HUDController.Singleton);
            HUDController.Singleton.PlayerCard.Init(playerId, character, avatar);
        }
    }
}
