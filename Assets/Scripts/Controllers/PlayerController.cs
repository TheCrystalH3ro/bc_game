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
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class PlayerController : NetworkBehaviour
    {
        public static PlayerController Singleton { get; private set; }

        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Animator animator;
        [SerializeField] private NetworkAnimator networkAnimator;

        [SerializeField] private RuntimeAnimatorController knightAnimator;
        [SerializeField] private RuntimeAnimatorController wizardAnimator;
        [SerializeField] private RuntimeAnimatorController rogueAnimator;

        [SerializeField] private TMPro.TextMeshPro playerName;

        [SerializeField] private GameObject playerStatusPrefab;

        [SerializeField] private Texture2D hoverCursor;

        private PlayerStatusController playerStatusController = null;

        private readonly SyncVar<PlayerCharacter> playerCharacter = new(new SyncTypeSettings());

        public static event System.Action OnEscapePressed;

        private MovementModule movementModule;

        private IParty party;

        public string ActiveScene { get; set; } = SceneModule.MAIN_SCENE_NAME;

        public override void OnStartNetwork()
        {
            movementModule = gameObject.GetComponent<MovementModule>();
            playerCharacter.OnChange += PlayerCharacterChanged;

            if (!base.Owner.IsLocalClient)
            {
                HoverPointerCursor hover = gameObject.AddComponent<HoverPointerCursor>();
                hover.SetPointerCursor(hoverCursor);
                return;
            }

            Singleton = this;

            base.SceneManager.OnLoadEnd += SceneChanged;
        }

        void OnDisable()
        {
            playerCharacter.OnChange -= PlayerCharacterChanged;

            if (!base.Owner.IsLocalClient)
                return;

            base.SceneManager.OnLoadEnd -= SceneChanged;
        }

        // Update is called once per frame
        void Update()
        {
            animator.SetFloat("Speed", movementModule.Movement.sqrMagnitude);
            bool isFlipped = GetComponent<SpriteRenderer>().flipX;

            if ((movementModule.Movement.x < 0 && !isFlipped) || (movementModule.Movement.x > 0 && isFlipped))
            {
                FlipCharacter(movementModule.Movement.x < 0);
            }

            if (!base.IsOwner) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEscapePressed?.Invoke();
            }
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

        private void FlipCharacter(bool isFlipped)
        {
            GetComponent<SpriteRenderer>().flipX = isFlipped;
        }

        public void SetPlayerCharacter(PlayerCharacter playerCharacter)
        {
            this.playerCharacter.Value = playerCharacter;
        }

        private void PlayerCharacterChanged(PlayerCharacter prev, PlayerCharacter next, bool asServer)
        {
            animator.runtimeAnimatorController = GetCharacterAnimatorController(next.GetPlayerClass());
            playerName.text = next.GetName();

            if (!IsOwner) return;

            animator.Update(0);

            if (playerStatusController == null)
            {
                GameObject playerHud = GameObject.FindGameObjectWithTag("PlayerStatus");
                GameObject playerStatus = Instantiate(playerStatusPrefab, playerHud.transform);

                playerStatusController = playerStatus.GetComponent<PlayerStatusController>();
            }

            playerStatusController.Init(next, gameObject.GetComponent<SpriteRenderer>().sprite, 100, 100);

            playerStatusController.UpdateHealth(next.GetHealth());
            playerStatusController.UpdateExp(next.GetExp());
        }

        public PlayerCharacter GetPlayerCharacter()
        {
            return playerCharacter.Value;
        }

        private RuntimeAnimatorController GetCharacterAnimatorController(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Wizard => wizardAnimator,
                PlayerClass.Rogue => rogueAnimator,
                _ => knightAnimator,
            };
        }

        public void OnMouseDown()
        {
            if (IsOwner) return;

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
    }
}
