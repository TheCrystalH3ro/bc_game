using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Models
{
    public class HealthModule : MonoBehaviour
    {
        [SerializeField] private int maxHp;
        private int currentHp;

        public UnityEvent<int> OnHeal;
        public UnityEvent<int> OnHurt;
        public UnityEvent OnDeath;

        void OnAwake()
        {
            currentHp = maxHp;
        }

        public int GetMaxHP()
        {
            return maxHp;
        }

        public void SetMaxHP(int maxHp)
        {
            this.maxHp = maxHp;
        }

        public int GetHP()
        {
            return currentHp;
        }

        public void SetHP(int hp)
        {
            currentHp = Mathf.Clamp(hp, 0, maxHp);

            if(hp < 0)
            {
                OnHurt?.Invoke(hp);

                if(currentHp <= 0)
                    OnDeath?.Invoke();
            }

            if(hp > 0)
                OnHeal?.Invoke(hp);
        }

        public void AddHP(int amount)
        {
            SetHP(currentHp + amount);
        }

        public void TakeHP(int amount)
        {
            SetHP(currentHp - amount);
        }
    }
}
