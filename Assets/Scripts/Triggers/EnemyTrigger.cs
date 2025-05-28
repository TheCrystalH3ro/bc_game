using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Modules;
using Assets.Scripts.UI.Controllers;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Triggers
{
    public class EnemyTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!InstanceFinder.NetworkManager.IsServerOnlyStarted)
                return;

            if (!other.CompareTag("Player") || !other.TryGetComponent<PlayerController>(out var playerObject))
                    return;

            if (!gameObject.TryGetComponent<EnemyController>(out var enemy))
                return;

            CombatModule.Singleton.StartCombat(playerObject, enemy.Character);
        }
    }
}