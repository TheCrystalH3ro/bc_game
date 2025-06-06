using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Models;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Modules
{
    public class SceneModule
    {
        private static SceneModule _instance;

        public static SceneModule Singleton
        {
            get
            {
                _instance ??= new();

                return _instance;
            }
        }

        public static readonly string MAIN_SCENE_NAME = "Lobby";
        public static readonly string DEFAULT_SCENE_NAME = "Town";

        public static readonly LocalPhysicsMode physicsMode = LocalPhysicsMode.Physics2D;

        private readonly HashSet<NetworkConnection> _processingPlayers = new();
        private readonly Dictionary<NetworkConnection, SceneChangeData> _playerSceneData = new();
        private readonly Dictionary<int, Action<int>> _instanceLoadEvents = new();

        private int lastInstanceId = -1;

        public static event Action<NetworkConnection, string, string> SceneChanged;

        public SceneModule()
        {
            if (!InstanceFinder.IsServerStarted)
                return;

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            InstanceFinder.SceneManager.OnLoadStart += OnSceneLoadStart;
            InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;
        }

        public void LoadStartScene()
        {
            SceneLoadData sld = new(MAIN_SCENE_NAME);

            sld.Options.AutomaticallyUnload = false;

            InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        }

        public void ChangeScene(NetworkConnection player, string sceneName, Vector3 startPosition = new())
        {
            if (_processingPlayers.Contains(player))
                return;

            NetworkObject playerObject = player.FirstObject;
            NetworkObject[] objectsToMove = new NetworkObject[] { playerObject };

            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            string activeScene = playerController.ActiveScene;

            _playerSceneData.Add(player, new(playerObject, startPosition, sceneName, activeScene, -1));

            LoadScene(player, sceneName, objectsToMove, false);
            UnloadScene(player, activeScene);

            playerController.ActiveScene = sceneName;
        }

        private void LoadScene(NetworkConnection player, string sceneName, NetworkObject[] objectsToMove = null, bool allowStacking = false)
        {
            LoadScene(new List<NetworkConnection>() { player }, sceneName, objectsToMove, allowStacking);
        }

        private void LoadScene(List<NetworkConnection> players, string sceneName, NetworkObject[] objectsToMove = null, bool allowStacking = false)
        {
            SceneLoadData sld = new(sceneName);

            sld.ReplaceScenes = ReplaceOption.All;
            sld.MovedNetworkObjects = objectsToMove;
            sld.Options.LocalPhysics = physicsMode;
            sld.Options.AllowStacking = allowStacking;
            sld.Options.AutomaticallyUnload = false;

            InstanceFinder.SceneManager.LoadConnectionScenes(players.ToArray(), sld);
        }

        private void UnloadScene(NetworkConnection player, string sceneName)
        {
            UnloadScene(new List<NetworkConnection>() { player }, sceneName);
        }
        
        private void UnloadScene(NetworkConnection player, int sceneHandle)
        {
            UnloadScene(new List<NetworkConnection>() { player }, sceneHandle);
        }

        private void UnloadScene(List<NetworkConnection> players, string sceneName)
        {
            SceneUnloadData sld = new(sceneName);
            sld.Options.Mode = UnloadOptions.ServerUnloadMode.KeepUnused;
            InstanceFinder.SceneManager.UnloadConnectionScenes(players.ToArray(), sld);
        }
        
        private void UnloadScene(List<NetworkConnection> players, int sceneHandle)
        {
            SceneUnloadData sld = new(sceneHandle);
            sld.Options.Mode = UnloadOptions.ServerUnloadMode.UnloadUnused;
            InstanceFinder.SceneManager.UnloadConnectionScenes(players.ToArray(), sld);
        }

        public void LoadInstance(List<NetworkConnection> players, string sceneName, Vector3 startPosition = new(), Action<int> instanceLoadEvent = null)
        {
            List<NetworkConnection> playersToMove = new();
            List<NetworkObject> playerObjects = new();

            int instanceId = ++lastInstanceId;

            if (instanceLoadEvent != null)
            {
                _instanceLoadEvents.Add(instanceId, instanceLoadEvent);
            }

            string activeScene = "";

            foreach (NetworkConnection player in players)
            {
                if (_processingPlayers.Contains(player))
                    continue;

                playersToMove.Add(player);

                NetworkObject playerObject = player.FirstObject;
                playerObjects.Add(playerObject);

                PlayerController playerController = playerObject.GetComponent<PlayerController>();
                activeScene = playerController.ActiveScene;
                _playerSceneData.Add(player, new(playerObject, startPosition, sceneName, activeScene, instanceId));
            }

            NetworkObject[] objectsToMove = playerObjects.ToArray();

            LoadScene(playersToMove, sceneName, objectsToMove, true);
            UnloadScene(playersToMove, activeScene);
        }

        public void LeaveInstance(List<NetworkConnection> players, Vector3 startPosition = new())
        {
            List<NetworkObject> playerObjects = new();

            string sceneName = "";
            int sceneHandle = players.First().FirstObject.gameObject.scene.handle;

            foreach (NetworkConnection player in players)
            {
                NetworkObject playerObject = player.FirstObject;
                playerObjects.Add(playerObject);

                PlayerController playerController = playerObject.GetComponent<PlayerController>();
                sceneName = playerController.ActiveScene;

                string activeScene = playerObject.gameObject.scene.name;

                _playerSceneData.Add(player, new(playerObject, startPosition, sceneName, activeScene, -1));
            }

            NetworkObject[] objectsToMove = playerObjects.ToArray();

            LoadScene(players, sceneName, objectsToMove, false);
            UnloadScene(players, sceneHandle);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool isPhysics3D = scene.GetPhysicsScene() != Physics.defaultPhysicsScene;
            bool isPhysics2D = scene.GetPhysicsScene2D() != Physics2D.defaultPhysicsScene;

            if (!isPhysics3D && !isPhysics2D)
                return;

            GameObject physicSync = new("PhysicsSceneSync");

            var sync = physicSync.AddComponent<PhysicsSceneSync>();
            sync._synchronizePhysics = isPhysics3D;
            sync._synchronizePhysics2D = isPhysics2D;

            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(physicSync, scene);
        }

        private void OnSceneLoadStart(SceneLoadStartEventArgs args)
        {
            NetworkConnection[] connections = args.QueueData.Connections;

            foreach (NetworkConnection connection in connections)
            {
                _processingPlayers.Add(connection);
            }
        }

        private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
        {
            NetworkConnection[] connections = args.QueueData.Connections;

            foreach (NetworkConnection connection in connections)
            {
                if (_processingPlayers.Contains(connection))
                {
                    _processingPlayers.Remove(connection);
                }

                if (_playerSceneData.ContainsKey(connection))
                {
                    SceneChangeData sceneData = _playerSceneData[connection];
                    sceneData.player.transform.position = sceneData.position;

                    _playerSceneData.Remove(connection);

                    SceneChanged?.Invoke(connection, sceneData.scene, sceneData.previousScene);

                    if (sceneData.instanceId >= 0 && _instanceLoadEvents.ContainsKey(sceneData.instanceId))
                    {
                        Action<int> instanceLoadEvent = _instanceLoadEvents[sceneData.instanceId];
                        instanceLoadEvent?.Invoke(sceneData.player.gameObject.scene.handle);
                        _instanceLoadEvents.Remove(sceneData.instanceId);
                    }
                }
            }
        }
    }
}