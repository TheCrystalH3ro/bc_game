using System;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers.Server
{
    public class EnemyController : BaseCharacterController
    {
        [SerializeField] private string enemyName;
        [SerializeField] private int enemyLevel;
        [SerializeField] private RuntimeAnimatorController hitAnimator;
        [SerializeField] private Texture2D hoverCursor;
        [SerializeField] private GameObject selectIndicator;

        public NetworkObject Prefab;

        public IEnemy Character { get; private set; }

        public bool IsSelectable { get; private set; }

        public uint Id { get; private set; }

        public static uint lastId = 0;

        public static event Action<EnemyController> EnemySpawned;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            Character = new Enemy(enemyName, enemyLevel);

            Id = lastId++;

            EnemySpawned?.Invoke(this);
        }

        public override bool Equals(BaseCharacterController other)
        {
            EnemyController otherEnemy = other as EnemyController;

            if (otherEnemy == null)
                return false;

            return Equals(otherEnemy);
        }

        public bool Equals(EnemyController enemy)
        {
            return this.Id == enemy.Id;
        }

        public override string ToString()
        {
            return Character.GetName();
        }

        public override CharacterData ToCharacterData()
        {
            return base.ToCharacterData(Id, Character.GetName(), Character.GetLevel());
        }

        public RuntimeAnimatorController GetHitAnimator()
        {
            return hitAnimator;
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

        public void OnMouseDown()
        {
            if (!IsSelectable)
                return;

            CombatController.Singleton.AttackTarget(Id);
        }
    }
}