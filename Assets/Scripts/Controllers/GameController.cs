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
            if (InstanceFinder.IsServerStarted)
                return;

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
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Controller");

            foreach (GameObject playerObject in playerObjects)
                DontDestroyOnLoad(playerObject);

            GameObject canvas = GameObject.FindGameObjectWithTag("MainCanvas");
            DontDestroyOnLoad(canvas);

            GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            DontDestroyOnLoad(mainCamera);

            GameObject eventSystem = FindFirstObjectByType<EventSystem>().gameObject;
            DontDestroyOnLoad(eventSystem);
        }

        private void DestroyObjects()
        {
            GameObject eventSystem = FindFirstObjectByType<EventSystem>().gameObject;
            Destroy(eventSystem);

            GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            Destroy(mainCamera);

            GameObject canvas = GameObject.FindGameObjectWithTag("MainCanvas");
            Destroy(canvas);

            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Controller");

            foreach (GameObject playerObject in playerObjects)
                Destroy(playerObject);
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
