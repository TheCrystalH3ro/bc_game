using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using Assets.Scripts.Responses;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Controllers.Server
{
    public class GameServerController : NetworkBehaviour
    {
        public static GameServerController Singleton { get; private set; }

        [SerializeField] private string apiUrl;

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject combatControllerPrefab;

        public Dictionary<uint, int> PlayerList { get; private set; } = new();

        void OnEnable()
        {
            Singleton = FindFirstObjectByType<GameServerController>();
        }

        void OnDisable()
        {
            SceneModule.SceneChanged -= OnSceneChanged;
        }

        public override void OnStartNetwork()
        {
            if (!base.IsServerInitialized)
            {
                GameController.Singleton.Initialize();
                return;
            }

            Debug.Log("Server initialized");

            SceneModule.Singleton.LoadStartScene();

            GameObject combatController = Instantiate(combatControllerPrefab);
            InstanceFinder.ServerManager.Spawn(combatController);

            SceneModule.SceneChanged += OnSceneChanged;
            InstanceFinder.ServerManager.OnRemoteConnectionState += HandlePlayerConnection;
        }

        private void HandlePlayerConnection(NetworkConnection client, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState.Equals(RemoteConnectionState.Stopped))
            {
                HandlePlayerDisconnected(client);
                return;
            }
        }

        private void HandlePlayerDisconnected(NetworkConnection client)
        {
            PlayerController playerController = PlayerController.FindByConnection(client);
            PlayerCharacter character = playerController.GetPlayerCharacter();

            if (character == null) return;

            if (playerController.ActiveScene != SceneModule.MAIN_SCENE_NAME)
            { 
                playerController.Save();
            }

            uint playerId = character.GetId();

            if (!PlayerList.ContainsKey(playerId)) return;

            PlayerList.Remove(playerId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestToJoinServer(string playerToken, uint characterId, NetworkConnection sender = null)
        {
            ConnectionModule.Singleton.VerifyPlayer(sender, playerToken, characterId, OnVerificationSuccess, OnVerificationFail);
        }

        private void OnVerificationSuccess(NetworkConnection client, CharacterResponse characterResponse)
        {
            PlayerCharacter playerCharacter = new(characterResponse);
            GameObject playerObject = SpawnPlayer(playerCharacter, client);
            SetupPlayer(playerObject, playerCharacter, client);
        }

        private void OnVerificationFail(NetworkConnection client, UnityWebRequest request)
        {
            ReturnToCharacterSelect(client);

            if (request.responseCode == 403)
            {
                Debug.LogError("Invalid character");
                client.Kick(KickReason.ExploitAttempt);
                return;
            }

            client.Kick(KickReason.UnexpectedProblem);
        }

        private void SetupPlayer(GameObject playerObject, PlayerCharacter playerCharacter, NetworkConnection client)
        {
            PlayerController playerController = playerObject.GetComponent<PlayerController>();

            int health = playerController.GetComponent<HealthModule>().GetHP();

            playerCharacter.SetHealth(health);

            playerController.SetPlayerCharacter(playerCharacter);
            
            playerController.Load(character =>
            {
                string currentScene = playerController.ActiveScene;
                playerController.ActiveScene = SceneModule.MAIN_SCENE_NAME;

                SceneModule.Singleton.ChangeScene(client, currentScene, playerObject.transform.position);
            }, error =>
            {
                client.Kick(KickReason.UnexpectedProblem);
            });
        }

        private GameObject SpawnPlayer(PlayerCharacter playerCharacter, NetworkConnection client)
        {
            var playerObject = Instantiate(playerPrefab);

            NetworkObject playerNetworkObject = playerObject.GetComponent<NetworkObject>();

            InstanceFinder.ServerManager.Spawn(playerNetworkObject, client);

            client.SetFirstObject(playerNetworkObject);

            PlayerList[playerCharacter.GetId()] = client.ClientId;

            return playerObject;
        }

        private void OnSceneChanged(NetworkConnection client, string sceneName, string previousScene)
        {
            PlayerController playerController = PlayerController.FindByConnection(client);
            playerController.OnZoneChange(sceneName);

            if (previousScene == SceneModule.MAIN_SCENE_NAME)
                return;

            playerController.Save();
        }

        [TargetRpc]
        private void ReturnToCharacterSelect(NetworkConnection client)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelect", LoadSceneMode.Single);
        } 
    }
}
