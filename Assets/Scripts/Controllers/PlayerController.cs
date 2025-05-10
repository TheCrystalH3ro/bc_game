using System.Collections;
using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using Assets.Scripts.UI.Controllers;
using Cinemachine;
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class PlayerController : NetworkBehaviour
    {
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

        private Vector3 initScale;

        private readonly SyncVar<PlayerCharacter> playerCharacter = new(new SyncTypeSettings());

        public static event System.Action OnEscapePressed;

        private MovementModule movementModule;

        private Party party;

        private void Awake()
        {
            playerCharacter.OnChange += PlayerCharacterChanged;
        }

        public override void OnStartNetwork()
        {
            movementModule = gameObject.GetComponent<MovementModule>();

            initScale = transform.localScale;

            if(!base.Owner.IsLocalClient)
            {
                HoverPointerCursor hover = gameObject.AddComponent<HoverPointerCursor>();
                hover.SetPointerCursor(hoverCursor);
                return;
            }

            CinemachineVirtualCamera virtualCamera = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }

        // Update is called once per frame
        void Update()
        {
            animator.SetFloat("Speed", movementModule.Movement.sqrMagnitude);
            bool isFlipped = GetComponent<SpriteRenderer>().flipX;

            if((movementModule.Movement.x < 0 && !isFlipped) || (movementModule.Movement.x > 0 && isFlipped))
            {
                FlipCharacter(movementModule.Movement.x < 0);
            }

            if(!base.IsOwner) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEscapePressed?.Invoke();
            }
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
            animator.runtimeAnimatorController = GetCharacterAnimatorController(playerCharacter.Value.GetPlayerClass());
            playerName.text = playerCharacter.Value.GetName();

            if(!IsOwner) return;

            animator.Update(0);

            if(playerStatusController == null)
            {
                GameObject playerHud = GameObject.FindGameObjectWithTag("PlayerStatus");
                GameObject playerStatus = Instantiate(playerStatusPrefab, playerHud.transform);

                playerStatusController = playerStatus.GetComponent<PlayerStatusController>();
            }

            playerStatusController.Init(playerCharacter.Value, gameObject.GetComponent<SpriteRenderer>().sprite, 100, 100);

            playerStatusController.UpdateHealth(90);
            playerStatusController.UpdateExp(15);
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
            if(IsOwner) return;

            int playerId = OwnerId;
            PlayerCharacter playerCharacter = GetPlayerCharacter();
            Sprite avatar = gameObject.GetComponent<SpriteRenderer>().sprite;
        
            GameController.Singleton.InspectPlayer(playerId, playerCharacter, avatar);
        }

        public Party GetParty() {
            return party;
        }

        public void SetParty(Party party) {
            this.party = party;
        }
    }
}
