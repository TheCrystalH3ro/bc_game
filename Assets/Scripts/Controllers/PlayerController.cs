using System;
using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using Assets.Scripts.Responses;
using Assets.Scripts.UI.Controllers;
using Assets.Scripts.Util;
using Cinemachine;
using FishNet.Component.Animating;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class PlayerController : BaseCharacterController
    {
        public static PlayerController Singleton { get; private set; }

        private Rigidbody2D rb;
        private NetworkAnimator networkAnimator;

        [SerializeField] private TMPro.TextMeshPro playerName;

        [SerializeField] private GameObject playerStatusPrefab;

        [SerializeField] private Texture2D hoverCursor;

        private PlayerStatusController playerStatusController = null;

        private readonly SyncVar<PlayerCharacter> playerCharacter = new(new SyncTypeSettings());

        public static event Action OnEscapePressed;

        private MovementModule movementModule;

        private IParty party;

        protected bool IsInCombat = false;

        public string ActiveScene { get; set; } = SceneModule.MAIN_SCENE_NAME;

        public static event Action<PlayerController> PlayerSpawned;
        public static event Action<PlayerController, string> ZoneChanged;

        void Awake()
        {
            rb = gameObject.GetComponent<Rigidbody2D>();
            animator = gameObject.GetComponent<Animator>();
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            networkAnimator = gameObject.GetComponent<NetworkAnimator>();
        }

        public override void OnStartNetwork()
        {
            movementModule = gameObject.GetComponent<MovementModule>();
            playerCharacter.OnChange += PlayerCharacterChanged;

            PlayerSpawned?.Invoke(this);

            if (!base.Owner.IsLocalClient)
            {
                if (IsInCombat)
                    return;

                if (!gameObject.TryGetComponent<HoverPointerCursor>(out var hover))
                {
                    hover = gameObject.AddComponent<HoverPointerCursor>();
                }

                hover.SetPointerCursor(hoverCursor);

                return;
            }

            Singleton = this;

            base.SceneManager.OnLoadEnd += SceneChanged;
            base.SceneManager.OnQueueEnd += LoadQueueEnded;
        }

        void OnDisable()
        {
            playerCharacter.OnChange -= PlayerCharacterChanged;

            if (!base.Owner.IsLocalClient)
                return;

            base.SceneManager.OnLoadEnd -= SceneChanged;
            base.SceneManager.OnQueueEnd -= LoadQueueEnded;
        }

        // Update is called once per frame
        void Update()
        {
            if (!base.IsOwner) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEscapePressed?.Invoke();
            }
        }

        public override bool Equals(BaseCharacterController other)
        {
            PlayerController otherPlayer = other as PlayerController;

            if (otherPlayer == null)
                return false;

            return Equals(otherPlayer);
        }

        public bool Equals(PlayerController other)
        {
            return playerCharacter.Value.Equals(other.GetPlayerCharacter());
        }

        public override string ToString()
        {
            return playerCharacter.Value.GetName();
        }

        public static PlayerController FindByConnection(NetworkConnection connection)
        {
            return ObjectUtil.FindFirstByType<PlayerController>(connection.Objects);
        }

        private void SceneChanged(SceneLoadEndEventArgs args)
        {
            GameObject camera = GameObject.FindGameObjectWithTag("VirtualCamera");

            if (camera == null) return;

            CinemachineVirtualCamera virtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;

            CinemachineConfiner2D cameraConfiner = camera.GetComponent<CinemachineConfiner2D>();
            cameraConfiner.m_BoundingShape2D = GameObject.FindGameObjectWithTag("CameraBoundary").GetComponent<Collider2D>();
        }

        private void LoadQueueEnded()
        {
            LoadPlayerStatus(playerCharacter.Value);
        }

        public void SetPlayerCharacter(PlayerCharacter playerCharacter)
        {
            this.playerCharacter.Value = playerCharacter;
        }

        private void PlayerCharacterChanged(PlayerCharacter prev, PlayerCharacter next, bool asServer)
        {
            animator.runtimeAnimatorController = ClassAnimationController.Singleton.GetCharacterAnimatorController(next.GetPlayerClass());

            if (playerName != null)
            {
                playerName.text = next.GetName();
            }

            if (!IsOwner) return;

            animator.Update(0);

            LoadPlayerStatus(next);
        }

        private void LoadPlayerStatus(PlayerCharacter character)
        {
            GameObject playerHud = GameObject.FindGameObjectWithTag("PlayerStatus");

            if (playerHud == null)
                return;

            if ((playerStatusController == null || FindFirstObjectByType<PlayerStatusController>() == null) && playerStatusPrefab != null)
            {
                GameObject playerStatus = Instantiate(playerStatusPrefab, playerHud.transform);

                playerStatusController = playerStatus.GetComponent<PlayerStatusController>();
            }

            HealthModule healthModule = GetComponent<HealthModule>();

            playerStatusController.Init(character, gameObject.GetComponent<SpriteRenderer>().sprite, healthModule.GetMaxHP(), 100);

            playerStatusController.UpdateHealth(healthModule.GetHP());
            playerStatusController.UpdateExp(character.GetExp());
        }

        public PlayerCharacter GetPlayerCharacter()
        {
            return playerCharacter.Value;
        }

        public void OnMouseDown()
        {
            if (IsOwner || IsInCombat) return;

            int playerId = OwnerId;
            PlayerCharacter playerCharacter = GetPlayerCharacter();
            Sprite avatar = gameObject.GetComponent<SpriteRenderer>().sprite;

            GameController.Singleton.InspectPlayer(playerId, playerCharacter, avatar);
        }

        public IParty GetParty()
        {
            return party;
        }

        public void SetParty(IParty party)
        {
            this.party = party;
        }

        [Server]
        public void Load(Action<PlayerCharacter> onLoadSuccess = null, Action<string> onLoadFail = null)
        {
            StartCoroutine(GameDataModule.Singleton.LoadPlayerData(playerCharacter.Value.GetId(), playerData =>
            {
                if (playerCharacter.Value.GetId() != playerData.id)
                    return;

                playerData.worldData ??= new(SceneModule.DEFAULT_SCENE_NAME, new(Vector3.zero));
                playerData.worldData.scene ??= SceneModule.DEFAULT_SCENE_NAME;
                playerData.worldData.position ??= new(Vector3.zero);

                playerCharacter.Value.LoadData(playerData);
                ActiveScene = playerData.worldData.scene;

                PositionData pos = playerData.worldData.position;
                transform.position = new Vector3(pos.x, pos.y, pos.z);

                onLoadSuccess?.Invoke(playerCharacter.Value);
            },
            error =>
            {
                Debug.Log("Error while trying to save the character: " + error);
                onLoadFail?.Invoke(error);
            }));
        }

        [Server]
        public void Save(Action onSave = null)
        {
            IPlayerData saveData = new PlayerData(playerCharacter.Value, transform.position, ActiveScene);

            StartCoroutine(GameDataModule.Singleton.SavePlayerData(playerCharacter.Value.GetId(), saveData, () =>
            {
                onSave?.Invoke();
            },
            error =>
            {
                Debug.Log("Error while trying to save the character: " + error);
            }));
        }

        public void EnterCombatRpc()
        {
            if (IsServerInitialized && !IsOwner)
                EnterCombatRpc(Owner);

            EnterCombat();
        }

        public void LeaveCombatRpc()
        {
            if (IsServerInitialized && !IsOwner)
                LeaveCombatRpc(Owner);

            LeaveCombat();
        }

        [TargetRpc]
        [ObserversRpc]
        public void EnterCombatRpc(NetworkConnection networkConnection)
        {
            EnterCombat();
        }

        [TargetRpc]
        [ObserversRpc]
        public void LeaveCombatRpc(NetworkConnection networkConnection)
        {
            LeaveCombat();
        }

        public override void EnterCombat()
        {
            IsInCombat = true;

            if (gameObject.TryGetComponent<HoverPointerCursor>(out var hover))
            {
                hover.SetPointerCursor(null);
            }

            base.EnterCombat();
        }

        public override void LeaveCombat()
        {
            IsInCombat = false;

            if (gameObject.TryGetComponent<HoverPointerCursor>(out var hover))
            {
                hover.SetPointerCursor(hoverCursor);
            }

            base.LeaveCombat();
        }

        public override CharacterData ToCharacterData()
        {
            uint id = playerCharacter.Value.GetId();
            string name = playerCharacter.Value.GetName();
            int level = playerCharacter.Value.GetLevel();

            Sprite sprite = playerCharacter.Value.GetSprite();

            return base.ToCharacterData(id, name, level, sprite);
        }

        public void RespawnPlayer()
        {
            RespawnPlayerRpc(Owner);

            OnRespawn();
        }

        [TargetRpc]
        private void RespawnPlayerRpc(NetworkConnection client)
        {
            OnRespawn();
        }

        public void OnZoneChange(string zoneName)
        {
            ZoneChanged?.Invoke(this, zoneName);
            playerCharacter.Value.SetCurrentScene(zoneName);
        }
    }
}
