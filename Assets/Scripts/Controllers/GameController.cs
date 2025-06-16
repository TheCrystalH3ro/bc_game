using System.Linq;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using Assets.Scripts.UI.Controllers;
using FishNet;
using FishNet.Transporting;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class GameController : MonoBehaviour
    {
        public static GameController Singleton { get; private set; }

        private void RegisterEvents()
        {
            InstanceFinder.SceneManager.OnQueueStart += LoadQueueStarted;
            InstanceFinder.SceneManager.OnQueueEnd += LoadQueueEnded;
            InstanceFinder.ClientManager.OnClientConnectionState += OnConnectionStop;
            PlayerController.OnEscapePressed += TogglePauseMenu;
        }

        void OnDisable()
        {
            if (InstanceFinder.NetworkManager == null || InstanceFinder.NetworkManager.IsServerStarted)
                return;

            InstanceFinder.SceneManager.OnQueueStart -= LoadQueueStarted;
            InstanceFinder.SceneManager.OnQueueEnd -= LoadQueueEnded;
            InstanceFinder.ClientManager.OnClientConnectionState -= OnConnectionStop;
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
            uint characterId = (uint)PlayerPrefs.GetInt("CharacterId");

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
            HUDController.Singleton.PlayerCard.Init(playerId, character, avatar);
        }

        public void CancelConnection()
        {
            if (InstanceFinder.IsServerStarted)
            {
                GameServerController.Singleton.StopServer();
                return;
            }

            Disconnect();
        }

        private void OnConnectionStop(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState != LocalConnectionState.Stopped)
                return;

            Disconnect();
        }

        public void Disconnect()
        {
            if (InstanceFinder.IsClientStarted)
                ConnectionModule.Singleton.Disconnect();

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelect", UnityEngine.SceneManagement.LoadSceneMode.Single);
            Destroy(this);
        }
    }
}
