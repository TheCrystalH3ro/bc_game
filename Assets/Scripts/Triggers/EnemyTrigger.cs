using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.UI.Controllers;
using FishNet;
using UnityEngine;

namespace Assets.Scripts.Triggers
{
    public class EnemyTrigger : MonoBehaviour
    {
        private bool isTriggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTriggered)
                return;

            if (!other.CompareTag("Player") || !other.TryGetComponent<PlayerController>(out var playerObject))
                return;

            isTriggered = true;

            if (!InstanceFinder.NetworkManager.IsServerStarted)
            {
                if (playerObject.Equals(PlayerController.Singleton))
                    HUDController.Singleton.ShowLoadingScreen();
                    
                return;
            }

            if (!gameObject.TryGetComponent<EnemyController>(out var enemy))
                return;

            CombatServerController.Singleton.MoveToCombat(playerObject, enemy);
        }
        
        public void SetActive(bool isActive)
        {
            this.isTriggered = !isActive;
        }
    }
}