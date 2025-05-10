using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Models
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHp;
        private int currentHp;

        public UnityEvent<int> OnHeal;
        public UnityEvent<int> OnHurt;
        public UnityEvent OnDeath;

        void OnAwake() {
            currentHp = maxHp;
        }

        public int GetMaxHp() {
            return maxHp;
        }

        public void SetMaxHp(int maxHp) {
            this.maxHp = maxHp;
        }

        public int GetHp() {
            return currentHp;
        }

        public void SetHp(int hp) {
            currentHp = Mathf.Clamp(hp, 0, maxHp);

            if(hp < 0) {
                OnHurt?.Invoke(hp);

                if(currentHp <= 0) {
                    OnDeath?.Invoke();
                }
            }

            if(hp > 0) {
                OnHeal?.Invoke(hp);
            }
        }

        public void AddHp(int amount) {
            SetHp(currentHp + amount);
        }

        public void TakeHp(int amount) {
            SetHp(currentHp - amount);
        }
    }
}
