using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class CharacterMovementModule : MovementModule
    {
        private Animator animator;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            animator = GetComponent<Animator>();

            TimeManager.OnTick += OnTick;
        }

        public override void OnStopNetwork()
        {
            base.TimeManager.OnTick -= OnTick;
        }

        public virtual void OnTick()
        {
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (!base.IsOwner && !base.IsServerInitialized)
                return;

            animator.SetFloat("Speed", Movement.sqrMagnitude);

            bool isFlipped = GetComponent<SpriteRenderer>().flipX;

            if ((Movement.x < 0 && !isFlipped) || (Movement.x > 0 && isFlipped))
            {
                if (base.IsServerInitialized)
                {
                    FlipCharacterRpc(Movement.x < 0);
                }
                
                FlipCharacter(Movement.x < 0);
            }
        }

        [ObserversRpc(ExcludeOwner = true)]
        private void FlipCharacterRpc(bool isFlipped)
        {
            FlipCharacter(isFlipped);
        }

        private void FlipCharacter(bool isFlipped)
        {
            GetComponent<SpriteRenderer>().flipX = isFlipped;
        }
    }
}