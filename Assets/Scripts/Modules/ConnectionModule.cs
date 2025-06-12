using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Responses;
using FishNet;
using FishNet.Connection;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Modules
{
    public class ConnectionModule
    {
        private static ConnectionModule _instance;

        public static ConnectionModule Singleton
        {
            get
            {
                _instance ??= new(ConfigModule.Get("API_URL"));

                return _instance;
            }
        }

        private readonly string apiUrl;

        private string serverHost = ConfigModule.Get("SERVER_HOST", "localhost");
        private int serverPort = ConfigModule.GetInt("SERVER_PORT", 80);

        public ConnectionModule(string apiUrl)
        {
            this.apiUrl = apiUrl;
        }

        public void HostServer()
        {
            Debug.Log("Starting the server");
            InstanceFinder.ServerManager.StartConnection((ushort)serverPort);
        }

        public void JoinServer()
        {
            Debug.Log("Joining the server");
            InstanceFinder.ClientManager.StartConnection(serverHost, (ushort)serverPort);
        }

        public void Disconnect()
        {
            InstanceFinder.ClientManager.StopConnection();
            UnityEngine.Object.Destroy(InstanceFinder.ClientManager.gameObject);
        }

        public void VerifyPlayer(NetworkConnection client, string playerToken, uint characterId, Action<NetworkConnection, CharacterResponse> onVerificationSuccess, Action<NetworkConnection, UnityWebRequest> onVerificationFail)
        {
            string verifyEndpoint = $"character/{characterId}";

            RequestModule.Singleton.GetRequest<CharacterResponse>(verifyEndpoint, playerToken, successResponse =>
            {
                onVerificationSuccess?.Invoke(client, successResponse);
            }, errorResponse =>
            {
                onVerificationFail?.Invoke(client, errorResponse);
            });
        }

        public void GroupRpc(List<NetworkConnection> connections, Delegate rpcMethod, params object[] args)
        {
            object[] rpcParams = new object[args.Length + 1];
            
            rpcParams[0] = null;
            Array.Copy(args, 0, rpcParams, 1, args.Length);
            
            foreach (NetworkConnection connection in connections)
            {
                rpcParams[0] = connection;
                rpcMethod.DynamicInvoke(rpcParams);
            }
        }
    }
}