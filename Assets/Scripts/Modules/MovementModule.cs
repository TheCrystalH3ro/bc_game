using FishNet.Managing.Timing;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class MovementModule : NetworkBehaviour
    {
        protected Rigidbody2D rb;

        public Vector2 Movement { get; private set; }

        public float moveSpeed = 5f;

        void Awake()
        {
            Movement = new Vector2(0, 0);

            rb = gameObject.GetComponent<Rigidbody2D>();
        }

        public void SetDirection(float x, float y)
        {
            Movement = new(x, y);
        }

        public void Move()
        {
            if(Movement.Equals(Vector2.zero) || moveSpeed == 0) return;

            float deltaTime = (float) TimeManager.TickDelta;

            Vector2 newPos = rb.position + (moveSpeed * deltaTime * Movement.normalized);

            rb.MovePosition(newPos);
        }
    }
}