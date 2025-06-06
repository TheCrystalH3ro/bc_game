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

        private Dictionary<uint, PlayerController> players = new();
        private Dictionary<uint, EnemyController> enemies = new();

        private Coroutine roundTimerCoroutine = null;

        void OnEnable()
        {
            PlayerController.PlayerSpawned += OnPlayerSpawned;
            EnemyController.EnemySpawned += OnEnemySpawned;
            CombatModule.CombatStarted += StartCombat;
        }

        void OnDisable()
        {
            PlayerController.PlayerSpawned -= OnPlayerSpawned;
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

            foreach (PlayerController player in players.Values)
            {
                PlacePlayer(player);
            }
        }

        public void StartCombat()
        {
            CombatUIController.Singleton.SetRoundTime(CombatModule.Instance.ROUND_TIME);
            CombatModule.Instance.RegisterOnTurnChangeEvent(TurnChanged);
        }

        private void PlacePlayer(PlayerController player)
        {
            GameObject[] playerSlots = GameObject.FindGameObjectsWithTag("PlayerSlot").OrderBy(slot => slot.name).ToArray();

            GameObject playerSlot = playerSlots[players.Count - 1];

            player.gameObject.transform.position = playerSlot.transform.position;

            player.FlipDirection(false);

            CombatUIController.Singleton.LoadCharacter(player);
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

        private void OnPlayerSpawned(PlayerController player)
        {
            players.Add(player.GetPlayerCharacter().GetId(), player);
            PlacePlayer(player);
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