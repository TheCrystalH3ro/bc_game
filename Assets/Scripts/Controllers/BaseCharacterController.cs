using System.Collections;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using Assets.Scripts.UI;
using Assets.Scripts.UI.Controllers;
using FishNet.Component.Transforming;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Controllers
{
    public abstract class BaseCharacterController : NetworkBehaviour
    {
        protected SpriteRenderer spriteRenderer;
        protected Animator animator;
        protected AttackModule attackModule;
        protected FloatTextModule floatTextModule;

        public Texture2D hoverCursor;
        [SerializeField] private GameObject selectIndicator;
        [SerializeField] private float whiteFlashDuration = 0.05f;
        [SerializeField] private float redFlashDuration = 0.1f;

        public bool IsSelectable { get; private set; }

        public UnityEvent<BaseCharacterController, BaseCharacterController> OnAttack;
        public UnityEvent<BaseCharacterController> OnStun;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            floatTextModule = GetComponent<FloatTextModule>();
        }

        public abstract bool Equals(BaseCharacterController other);
        public abstract bool Equals(Target target);
        public abstract override string ToString();
        public abstract Target ToTarget();

        public abstract uint GetId();

        public void FlipDirection(bool isFlipped)
        {
            if (gameObject.TryGetComponent<CharacterMovementModule>(out var movementModule))
            {
                movementModule.FlipCharacter(isFlipped);
            }
        }

        public void DisplayFloatText(string text, Color? color = null)
        {
            floatTextModule.DisplayFloatText(text, color ?? Color.white);
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
                healthModule.OnHurt.RemoveListener(OnHitReceived);
                healthModule.OnDeath.RemoveListener(OnDeath);

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

        protected void OnHitReceived(int amount, int hp)
        {
            DisplayFloatText( "- " + amount, Color.red);

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

        public void SetSelectable(bool selectable)
        {
            IsSelectable = selectable;

            if (!gameObject.TryGetComponent<HoverPointerCursor>(out var hover))
            {
                hover = gameObject.AddComponent<HoverPointerCursor>();
            }

            if (selectable)
            {
                selectIndicator.SetActive(true);
                hover.SetPointerCursor(hoverCursor);
                return;
            }

            selectIndicator.SetActive(false);
            hover.SetPointerCursor(null);
        }

        public virtual void OnMouseDown()
        {
            if (!IsSelectable)
                return;

            CombatController.Singleton.AttackTarget(GetId());
        }

        public abstract RuntimeAnimatorController GetHitAnimator();

        public int DoAttack(BaseCharacterController target, int damage)
        {
            OnAttack?.Invoke(this, target);

            return attackModule.DoAttack(target, damage);
        }

        public int GetDamage(FlashCard card, float remainingTime)
        {
            return attackModule.GetDamage(card, remainingTime);
        }

        public float GetDefense(FlashCard card, float remainingTime)
        {
            return attackModule.GetDefense(card, remainingTime);
        }

        public int BeforeDamage(int damage, BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            return attackModule.BeforeDamage(damage, target, answer, targetAnswer);
        }

        public int BeforeFinalDamage(int finalDamage, int originalDamage, float defense, BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            return attackModule.BeforeFinalDamage(finalDamage, originalDamage, defense, target, answer, targetAnswer);
        }

        public void AfterDamage(BaseCharacterController target, Answer answer, Answer targetAnswer)
        {
            attackModule.AfterDamage(target, answer, targetAnswer);
        }

        public float BeforeDefense(float defense, BaseCharacterController attacker, Answer answer, Answer attackerAnswer)
        {
            return attackModule.BeforeDefense(defense, attacker, answer, attackerAnswer);
        }

        public void AfterDefense(BaseCharacterController attacker, Answer answer, Answer attackerAnswer)
        {
            attackModule.AfterDefense(attacker, answer, attackerAnswer);
        }

        public void AddBuff(StatusEffectType buffType, float value, uint duration)
        {
            attackModule.AddBuff(buffType, value, duration);
        }

        public void UseBuff(StatusEffectType buff)
        {
            attackModule.UseBuff(buff);
        }

        public void UseBuffs()
        {
            attackModule.UseBuffs();
        }

        public void Stun()
        {
            OnStun?.Invoke(this);
            attackModule.Stun();
        }

        public bool IsStunned()
        {
            return attackModule.IsStunned();
        }

        public void UseStun()
        {
            OnStun?.Invoke(this);
            attackModule.UseStun();
        }
    }
}