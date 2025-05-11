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
            if(! base.IsServerInitialized) return;

            Debug.Log("Server initialized");

            InstanceFinder.ServerManager.OnRemoteConnectionState += HandlePlayerConnection;
        }

        private void HandlePlayerConnection(NetworkConnection client, RemoteConnectionStateArgs args)
        {
            if(! args.ConnectionState.Equals(RemoteConnectionState.Stopped)) return;

            PlayerCharacter character = client.FirstObject.GetComponent<PlayerCharacter>();

            if(character == null) return;

            uint playerId = character.GetId();

            if(! PlayerList.ContainsKey(playerId)) return;

            PlayerList.Remove(playerId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestToJoinServerRpc(string playerToken, uint characterId, NetworkConnection sender = null)
        {
            StartCoroutine(ConnectionModule.Singleton.VerifyPlayer(sender, playerToken, characterId, OnVerificationSuccess, OnVerificationFail));
        }

        private void OnVerificationSuccess(NetworkConnection client, CharacterResponse character)
        {
            SpawnPlayer(character, client);
        }

        private void OnVerificationFail(NetworkConnection client, UnityWebRequest request)
        {
            if (request.responseCode == 403) {
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
        }

        public void UpdatePartyStatus(Party party)
        {

        //     if(!IsServer) {
        //         return;
        //     }

        //     int partyMembersCount = party.GetMemberCount();
        //     PlayerCharacter[] partyMembers = party.GetMembers().Values.ToArray();
        //     ulong[] clientIds = party.GetClientIds();

        //     UpdatePartyRpc(partyMembers, party.GetPartyLeader().GetId(), RpcTarget.Group(clientIds, RpcTargetUse.Temp));
        // }

        // [TargetRpc]
        // private void UpdatePartyRpc(NetworkConnection client, PlayerCharacter[] playerCharacters, uint partyLeaderId, RpcParams rpcParams = default) {
        //     GameController.Singleton.UpdatePartyUI(playerCharacters.ToList(), partyLeaderId);
        }

        
    }
}
