using System;
using System.Collections.Generic;
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

            _playerSceneData.Add(player, new(playerObject, startPosition, sceneName, activeScene));

            LoadScene(player, sceneName, objectsToMove);
            UnloadScene(player, activeScene);

            playerController.ActiveScene = sceneName;
        }

        private void LoadScene(NetworkConnection player, string sceneName, NetworkObject[] objectsToMove = null)
        {
            SceneLoadData sld = new(sceneName);

            sld.ReplaceScenes = ReplaceOption.All;
            sld.MovedNetworkObjects = objectsToMove;
            sld.Options.LocalPhysics = physicsMode;
            sld.Options.AutomaticallyUnload = false;

            InstanceFinder.SceneManager.LoadConnectionScenes(player, sld);
        }

        private void UnloadScene(NetworkConnection player, string sceneName)
        {
            SceneUnloadData sld = new(sceneName);
            sld.Options.Mode = UnloadOptions.ServerUnloadMode.KeepUnused;
            InstanceFinder.SceneManager.UnloadConnectionScenes(player, sld);
        }

        public void LoadInstance(NetworkConnection player, string sceneName)
        {
            SceneLoadData sld = new(sceneName);

            sld.ReplaceScenes = ReplaceOption.All;

            sld.Options.AllowStacking = true;
            sld.Options.LocalPhysics = physicsMode;

            InstanceFinder.SceneManager.LoadConnectionScenes(player, sld);
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
                }
            }
        }
    }
}