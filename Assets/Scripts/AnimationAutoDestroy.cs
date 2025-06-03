using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class AnimationAutoDestroy : MonoBehaviour
    {
        private Animator animator;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            float length = GetAnimationLength();

            Destroy(gameObject, length);
        }

        private float GetAnimationLength()
        {
            RuntimeAnimatorController rac = animator.runtimeAnimatorController;

            return rac.animationClips.First().length;
        }
    }
}