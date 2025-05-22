using Assets.Scripts.Models;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class PlayerMovementModule : MovementModule
    {

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            TimeManager.OnTick += OnTick;
            TimeManager.OnPostTick += OnPostTick;
        }

        public override void OnStopNetwork()
        {
            base.TimeManager.OnTick -= OnTick;
            base.TimeManager.OnPostTick -= OnPostTick;
        }

        private void OnTick()
        {
            MovementReplicateData input = HandleMovementInput();
            UpdateMovement(input);
        }

        private void OnPostTick()
        {
            CreateReconcile();
        }

        private MovementReplicateData HandleMovementInput()
        {
            if (!base.IsOwner)
                return default;

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            return new()
            {
                direction = new(x, y)
            };
        }

        [Replicate]
        private void UpdateMovement(MovementReplicateData data, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            SetDirection(data.direction.x, data.direction.y);

            Move();
        }

        [ObserversRpc(ExcludeOwner = true, ExcludeServer = true)]
        public void ShareMovement(Vector2 direction)
        {
            SetDirection(direction.x, direction.y);
            Move();
        }

        [Reconcile]
        private void ReconcilePosition(MovementReconcileData data, Channel channel = Channel.Unreliable)
        {
            transform.position = data.position;
        }

        public override void CreateReconcile()
        {
            MovementReconcileData data = new(transform.position);
            ReconcilePosition(data);
        }
    }
}