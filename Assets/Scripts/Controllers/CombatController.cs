using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers.Server;
using FishNet;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class CombatController : MonoBehaviour
    {
        private List<PlayerController> players;
        private List<EnemyController> enemies;

        void OnEnable()
        {
            EnemyController.EnemySpawned += OnEnemySpawned;
        }

        void OnDisable()
        {
            EnemyController.EnemySpawned -= OnEnemySpawned;
        }

        void Start()
        {
            if (InstanceFinder.IsServerStarted)
                return;

            Initialize();
        }

        public void Initialize()
        {
            players = FindObjectsByType<PlayerController>(FindObjectsSortMode.InstanceID).ToList();
            enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.InstanceID).ToList();

            PlacePlayers();
        }

        private void PlacePlayers()
        {
            GameObject[] playerSlots = GameObject.FindGameObjectsWithTag("PlayerSlot");

            int index = 0;
            foreach (PlayerController player in players)
            {
                GameObject playerSlot = playerSlots[index];

                player.gameObject.transform.position = playerSlot.transform.position;

                player.FlipDirection(false);

                index++;
            }
        }

        private void OnEnemySpawned(EnemyController enemy)
        {
            enemy.FlipDirection(true);
        }
    }
}