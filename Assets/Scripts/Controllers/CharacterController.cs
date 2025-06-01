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
    }
}