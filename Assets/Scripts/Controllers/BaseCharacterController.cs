using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using FishNet.Component.Transforming;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public abstract class BaseCharacterController : NetworkBehaviour
    {
        public void FlipDirection(bool isFlipped)
        {
            if (gameObject.TryGetComponent<CharacterMovementModule>(out var movementModule))
            {
                movementModule.FlipCharacter(isFlipped);
            }
        }

        public void EnterCombat()
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
        }

        public void LeaveCombat()
        {
            if (gameObject.TryGetComponent<MovementModule>(out var movementModule))
            {
                movementModule.SetActive(true);
            }

            if (gameObject.TryGetComponent<NetworkTransform>(out var netTransform))
            {
                netTransform.SetSynchronizePosition(true);
            }
        }

        public virtual CharacterData ToCharacterData()
        {
            return ToCharacterData(0, "", 0);
        }

        public CharacterData ToCharacterData(uint id, string name, int level)
        {
            HealthModule healthModule = GetComponent<HealthModule>();
            int health = healthModule.GetHP();
            int maxHealth = healthModule.GetMaxHP();

            Sprite sprite = GetComponent<SpriteRenderer>().sprite;

            return new(id, name, level, health, maxHealth, sprite);
        }
    }
}