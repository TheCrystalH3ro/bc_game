using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Modules;
using Assets.Scripts.UI.Controllers;
using FishNet;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class CombatController : MonoBehaviour
    {
        private static CombatController _instance;

        public static CombatController Singleton
        {
            get
            {
                _instance ??= FindFirstObjectByType<CombatController>();

                return _instance;
            }
        }

        [SerializeField] private GameObject hitPrefab;

        private Dictionary<uint, PlayerController> players;
        private Dictionary<uint, EnemyController> enemies = new();

        private Coroutine roundTimerCoroutine = null;

        void OnEnable()
        {
            EnemyController.EnemySpawned += OnEnemySpawned;
            CombatModule.CombatStarted += StartCombat;
        }

        void OnDisable()
        {
            EnemyController.EnemySpawned -= OnEnemySpawned;
            CombatModule.CombatStarted -= StartCombat;
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

        public void StartCombat()
        {
            CombatUIController.Singleton.SetRoundTime(CombatModule.Instance.ROUND_TIME);
            CombatModule.Instance.RegisterOnTurnChangeEvent(TurnChanged);
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

        private IEnumerator RoundTimer()
        {
            int remainingTurnTime = CombatModule.Instance.ROUND_TIME;

            while (remainingTurnTime >= 0)
            {
                CombatUIController.Singleton.ChangeRoundTime(remainingTurnTime);

                yield return new WaitForSeconds(1f);

                remainingTurnTime--;
            }
        }

        private void TurnChanged(int prev, int next, bool asServer)
        {
            if (roundTimerCoroutine != null)
                StopCoroutine(roundTimerCoroutine);

            roundTimerCoroutine = StartCoroutine(RoundTimer());
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
            uint target = enemies.First().Key;

            CombatServerController.Singleton.AttackEnemy(target);
        }

        public void PlayerAttack(uint playerId, uint enemyId)
        {
            PlayerController player = players[playerId];
            EnemyController enemy = enemies[enemyId];

            RuntimeAnimatorController hitAnimator = ClassAnimationController.Singleton.GetCharacterAttackAnimatorController(player.GetPlayerCharacter().GetPlayerClass());
            hitPrefab.GetComponent<Animator>().runtimeAnimatorController = hitAnimator;
            hitPrefab.GetComponent<SpriteRenderer>().flipX = false;

            Instantiate(hitPrefab, enemy.transform);
        }

        public void EnemyAttack(uint enemyId, uint playerId)
        {
            EnemyController enemy = enemies[enemyId];
            PlayerController player = players[playerId];

            RuntimeAnimatorController hitAnimator = enemy.GetHitAnimator();
            hitPrefab.GetComponent<Animator>().runtimeAnimatorController = hitAnimator;
            hitPrefab.GetComponent<SpriteRenderer>().flipX = true;

            Instantiate(hitPrefab, player.transform);
        }
    }
}