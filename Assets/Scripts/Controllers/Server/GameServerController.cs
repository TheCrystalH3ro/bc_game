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

        public Dictionary<uint, int> PlayerList { get; private set; } = new();

        void OnEnable()
        {
            Singleton = FindFirstObjectByType<GameServerController>();
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

            uint playerId = character.GetId();

            if (!PlayerList.ContainsKey(playerId)) return;

            PlayerList.Remove(playerId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestToJoinServer(string playerToken, uint characterId, NetworkConnection sender = null)
        {
            StartCoroutine(ConnectionModule.Singleton.VerifyPlayer(sender, playerToken, characterId, OnVerificationSuccess, OnVerificationFail));
        }

        private void OnVerificationSuccess(NetworkConnection client, CharacterResponse character)
        {
            SpawnPlayer(character, client);
            SceneModule.Singleton.ChangeScene(client, "Town");
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

            Debug.LogError("Error while trying to verify character: " + request.error);
            client.Kick(KickReason.UnexpectedProblem);
        }

        private void SpawnPlayer(CharacterResponse characterResponse, NetworkConnection client)
        {
            var playerObject = Instantiate(playerPrefab);

            NetworkObject playerNetworkObject = playerObject.GetComponent<NetworkObject>();

            InstanceFinder.ServerManager.Spawn(playerNetworkObject, client);

            PlayerController playerController = playerObject.GetComponent<PlayerController>();

            PlayerCharacter playerCharacter = new(characterResponse);

            playerController.SetPlayerCharacter(playerCharacter);

            PlayerList[playerCharacter.GetId()] = client.ClientId;

            client.SetFirstObject(playerController.NetworkObject);
        }

        [TargetRpc]
        private void ReturnToCharacterSelect(NetworkConnection client)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelect", LoadSceneMode.Single);
        } 
    }
}
