using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.UI.Controllers;
using FishNet;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class CombatController : MonoBehaviour
    {
        public static CombatController Singleton { get; private set; }

        [SerializeField] private GameObject hitPrefab;

        private Dictionary<uint, PlayerController> players;
        private Dictionary<uint, EnemyController> enemies = new();

        void OnEnable()
        {
            EnemyController.EnemySpawned += OnEnemySpawned;
        }

        void OnDisable()
        {
            EnemyController.EnemySpawned -= OnEnemySpawned;
        }

        void Awake()
        {
            Singleton = this;
        }

        void Start()
        {
            if (InstanceFinder.IsServerStarted)
                return;

            Initialize();
        }

        public void Initialize()
        {
            players = FindObjectsByType<PlayerController>(FindObjectsSortMode.InstanceID).ToDictionary(player => player.GetPlayerCharacter().GetId());
            enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.InstanceID).ToDictionary(enemy => enemy.Id);

            PlacePlayers();
        }

        private void PlacePlayers()
        {
            GameObject[] playerSlots = GameObject.FindGameObjectsWithTag("PlayerSlot");

            int index = 0;
            foreach (PlayerController player in players.Values)
            {
                GameObject playerSlot = playerSlots[index];

                player.gameObject.transform.position = playerSlot.transform.position;

                player.FlipDirection(false);

                CombatUIController.Singleton.LoadCharacter(player);

                index++;
            }
        }

        private void OnEnemySpawned(EnemyController enemy)
        {
            enemies.Add(enemy.Id, enemy);
            enemy.FlipDirection(true);
            enemy.EnterCombat();
            CombatUIController.Singleton.LoadCharacter(enemy);
        }

        public void Attack()
        {
            uint target = enemies.First().Key    ;

            Debug.Log("Attacking enemy with id " + target);

            CombatServerController.Singleton.AttackEnemy(target);
        }

        public void PlayerAttack(uint playerId, uint enemyId)
        {
            PlayerController player = players[playerId];
            EnemyController enemy = enemies[enemyId];

            RuntimeAnimatorController hitAnimator = ClassAnimationController.Singleton.GetCharacterAttackAnimatorController(player.GetPlayerCharacter().GetPlayerClass());
            hitPrefab.GetComponent<Animator>().runtimeAnimatorController = hitAnimator;

            Instantiate(hitPrefab, enemy.transform);
        }
    }
}