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
                if (_instance == null)
                {
                    _instance = new(ConfigModule.Get("API_URL"));
                }

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

        public IEnumerator VerifyPlayer(NetworkConnection client, string playerToken, uint characterId, Action<NetworkConnection, CharacterResponse> onVerificationSuccess, Action<NetworkConnection, UnityWebRequest> onVerificationFail)
        {
            string verifyUrl = apiUrl + "/character/" + characterId;

            using (UnityWebRequest request = UnityWebRequest.Get(verifyUrl))
            {

                request.SetRequestHeader("Authorization", playerToken);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onVerificationFail?.Invoke(client, request);
                    yield break;
                }

                CharacterResponse characterResponse = CharacterResponse.CreateFromJSON(request.downloadHandler.text);

                onVerificationSuccess?.Invoke(client, characterResponse);
            }
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