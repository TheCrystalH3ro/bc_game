using System.Collections;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using FishNet.Component.Transforming;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public abstract class BaseCharacterController : NetworkBehaviour
    {
        protected SpriteRenderer spriteRenderer;
        protected Animator animator;

        [SerializeField] private float whiteFlashDuration = 0.05f;
        [SerializeField] private float redFlashDuration = 0.1f;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        public void FlipDirection(bool isFlipped)
        {
            if (gameObject.TryGetComponent<CharacterMovementModule>(out var movementModule))
            {
                movementModule.FlipCharacter(isFlipped);
            }
        }

        public virtual void EnterCombat()
        {
            if (gameObject.TryGetComponent<MovementModule>(out var movementModule))
            {
                movementModule.SetActive(false);
                GetComponent<Animator>().SetFloat("Speed", 0);
            }

            if (gameObject.TryGetComponent<NetworkTransform>(out var netTransform))
            {
                netTransform.SetSynchronizePosition(false);
            }

            if (gameObject.TryGetComponent<HealthModule>(out var healthModule))
            {
                healthModule.OnHurt.AddListener(OnHitReceived);
                healthModule.OnDeath.AddListener(OnDeath);
            }
        }

        public virtual void LeaveCombat()
        {
            if (gameObject.TryGetComponent<MovementModule>(out var movementModule))
            {
                movementModule.SetActive(true);
            }

            if (gameObject.TryGetComponent<NetworkTransform>(out var netTransform))
            {
                netTransform.SetSynchronizePosition(true);
            }

            if (gameObject.TryGetComponent<HealthModule>(out var healthModule))
            {
                healthModule.OnHurt.RemoveListener(OnHitReceived);
                healthModule.OnDeath.RemoveListener(OnDeath);
            }
        }

        public virtual CharacterData ToCharacterData()
        {
            return ToCharacterData(0, "", 0);
        }

        public CharacterData ToCharacterData(uint id, string name, int level, Sprite sprite = null)
        {
            HealthModule healthModule = GetComponent<HealthModule>();
            int health = healthModule.GetHP();
            int maxHealth = healthModule.GetMaxHP();

            if (sprite == null)
            {
                sprite = GetComponent<SpriteRenderer>().sprite;
            }

            return new(id, name, level, health, maxHealth, sprite);
        }

        protected void OnHitReceived(int hp)
        {
            StartCoroutine(HitAnimation());
        }

        private IEnumerator HitAnimation()
        {
            Color originalColor = Color.white;
            Color whiteFlash = new(0.9716981f, 0.4720986f, 0.6089923f, 1f);

            spriteRenderer.color = whiteFlash;
            yield return new WaitForSeconds(whiteFlashDuration);

            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(redFlashDuration);

            spriteRenderer.color = whiteFlash;
            yield return new WaitForSeconds(whiteFlashDuration);

            spriteRenderer.color = originalColor;
        }

        protected void OnDeath()
        {
            animator.SetTrigger("Death");
        }

        protected void OnRespawn()
        {
            animator.SetTrigger("Respawn");
        }
    }
}