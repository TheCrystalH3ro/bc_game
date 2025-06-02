using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Models
{
    public class HealthModule : NetworkBehaviour
    {
        [SerializeField] private int maxHp;
        private readonly SyncVar<int> currentHp = new(new SyncTypeSettings());

        public UnityEvent<int> OnHeal;
        public UnityEvent<int> OnHurt;
        public UnityEvent OnDeath;

        public override void OnStartNetwork()
        {
            currentHp.Value = maxHp;
            currentHp.OnChange += HPChanged;
        }

        void OnDisable()
        {
            currentHp.OnChange -= HPChanged;
        }

        private void HPChanged(int prev, int next, bool asServer)
        {
            if(next < prev)
            {
                OnHurt?.Invoke(next);

                if(next <= 0)
                    OnDeath?.Invoke();
            }

            if(next > prev)
                OnHeal?.Invoke(currentHp.Value);
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
            return currentHp.Value;
        }

        public void SetHP(int hp)
        {
            currentHp.Value = Mathf.Clamp(hp, 0, maxHp);
        }

        public void AddHP(int amount)
        {
            SetHP(currentHp.Value + amount);
        }

        public void TakeHP(int amount)
        {
            SetHP(currentHp.Value - amount);
        }
    }
}
