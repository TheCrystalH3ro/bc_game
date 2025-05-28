using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class RoamMovementModule : CharacterMovementModule
    {
        [SerializeField] private Collider2D roamArea;
        [SerializeField] private float minMoveDistance = 0.5f;
        [SerializeField] private float maxMoveDistance = 5f;
        [SerializeField] private float minIdleTime = 3f;
        [SerializeField] private float maxIdleTime = 10f;
        private static readonly Vector2[] roamDirections = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right,
            new Vector2(1, 1).normalized,
            new Vector2(1, -1).normalized,
            new Vector2(-1, 1).normalized,
            new Vector2(-1, -1).normalized
        };

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            HandleMovement();
        }

        public override void OnTick()
        {
            base.OnTick();

            if (!base.IsServerInitialized)
                return;

            if (!CanMove())
            {
                ChooseDirection();
            }

            Move();
        }

        public void SetRoamArea(Collider2D area)
        {
            roamArea = area;
        }

        private bool CanMove()
        {
            return CanMove(Movement.x, Movement.y);
        }

        private bool CanMove(float x, float y)
        {
            if (roamArea == null)
                return true;

            float deltaTime = (float) TimeManager.TickDelta;

            Vector2 direction = new(x, y);
            Vector2 target =  rb.position + (moveSpeed * deltaTime * direction.normalized);

            if (!roamArea.OverlapPoint(target))
                return false;

            return true;
        }

        private void ChooseDirection()
        {
            int randomDir = Random.Range(0, roamDirections.Length - 1);

            for (int i = 0; i < roamDirections.Length; i++)
            {
                int index = (randomDir + i) % roamDirections.Length;
                Vector2 direction = roamDirections[index];

                if (CanMove(direction.x, direction.y))
                {
                    SetDirection(direction.x, direction.y);
                    return;
                }
            }

            SetDirection(0, 0);
        }

        private void HandleMovement()
        {
            float minMoveTime = minMoveDistance / moveSpeed;
            float maxMoveTime = maxMoveDistance / moveSpeed;

            float moveTime = Random.Range(minMoveTime, maxMoveTime);
            float idleTime = Random.Range(minIdleTime, maxIdleTime);

            StartCoroutine(Roam(moveTime, idleTime));
        }

        private IEnumerator Roam(float moveTime, float idleTime)
        {
            ChooseDirection();

            yield return new WaitForSeconds(moveTime);

            StartCoroutine(Idle(idleTime));

            yield return null;
        }

        private IEnumerator Idle(float idleTime)
        {
            SetDirection(0, 0);

            yield return new WaitForSeconds(idleTime);

            HandleMovement();

            yield return null;
        }
    }
}