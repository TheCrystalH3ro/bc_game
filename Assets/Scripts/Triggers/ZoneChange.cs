using System.Collections.Generic;
using Assets.Scripts.Modules;
using Assets.Scripts.UI.Controllers;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Triggers
{
    public class ZoneChange : MonoBehaviour
    {
        [SerializeField] private string zone;
        [SerializeField] private Vector3 spawnPoint;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") || !other.TryGetComponent<NetworkObject>(out var playerObject))
                return;

            if (!InstanceFinder.NetworkManager.IsServerOnlyStarted)
            {
                HUDController.Singleton.ShowLoadingScreen();
                return;
            }

            NetworkConnection connection = playerObject.Owner;

            SceneModule.Singleton.ChangeScene(connection, zone, spawnPoint);
        }
    }
}