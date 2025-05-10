using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class MovementModule : NetworkBehaviour
    {
        protected Rigidbody2D rb;

        public Vector2 Movement => _movement.Value;

        public readonly SyncVar<Vector2> _movement = new(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));

        public float moveSpeed = 5f;

        void Awake()
        {
            _movement.Value = new Vector2(0, 0);

            rb = gameObject.GetComponent<Rigidbody2D>();
        }

        public override void OnStartNetwork()
        {
            
        }

        [ServerRpc(RunLocally = true)]
        public void SetDirection(float x, float y)
        {
            _movement.Value = new(x, y);
        }


        public void Move()
        {
            if(_movement.Value.Equals(Vector2.zero) || moveSpeed == 0) return;

            rb.MovePosition(rb.position + (moveSpeed * Time.fixedDeltaTime * _movement.Value));
        }

        public void MoveTo(Vector3 position)
        {
            gameObject.transform.position = position;
        }

        [ObserversRpc(ExcludeServer = true)]
        public void ReconcilePosition(Vector3 position)
        {
            Debug.Log("Pozicia bola rekoncilovana");
            MoveTo(position);
        }

        private void FixedUpdate()
        {
            Move();

            if(! IsServerInitialized) return;

            ReconcilePosition(gameObject.transform.position);
        }
    }
}