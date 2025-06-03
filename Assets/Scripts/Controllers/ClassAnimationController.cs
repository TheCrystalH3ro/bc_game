using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class ClassAnimationController : MonoBehaviour
    {
        public static ClassAnimationController Singleton { get; private set; }

        [SerializeField] private RuntimeAnimatorController knightAnimator;
        [SerializeField] private RuntimeAnimatorController wizardAnimator;
        [SerializeField] private RuntimeAnimatorController rogueAnimator;

        [SerializeField] private RuntimeAnimatorController knightAttackAnimator;
        [SerializeField] private RuntimeAnimatorController wizardAttackAnimator;
        [SerializeField] private RuntimeAnimatorController rogueAttackAnimator;

        void Awake()
        {
            Singleton = this;
        }

        public RuntimeAnimatorController GetCharacterAnimatorController(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Wizard => wizardAnimator,
                PlayerClass.Rogue => rogueAnimator,
                _ => knightAnimator,
            };
        }

        public RuntimeAnimatorController GetCharacterAttackAnimatorController(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Wizard => wizardAttackAnimator,
                PlayerClass.Rogue => rogueAttackAnimator,
                _ => knightAttackAnimator,
            };
        }
    }
}