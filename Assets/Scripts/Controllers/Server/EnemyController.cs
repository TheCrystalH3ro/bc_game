using System;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Modules.Attack;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Controllers.Server
{
    public class EnemyController : BaseCharacterController
    {
        [SerializeField] private string enemyName;
        [SerializeField] private int enemyLevel;
        [SerializeField] private RuntimeAnimatorController hitAnimator;

        public NetworkObject Prefab;

        public IEnemy Character { get; private set; }

        public uint Id { get; private set; }

        public static uint lastId = 0;

        public static event Action<EnemyController> EnemySpawned;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            attackModule = GetComponent<EnemyAttackModule>();

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

        public override bool Equals(Target target)
        {
            if (!target.IsBot)
                return false;

            return Id == target.Id;
        }

        public bool Equals(EnemyController enemy)
        {
            return this.Id == enemy.Id;
        }

        public override string ToString()
        {
            return Character.GetName();
        }

        public override Target ToTarget()
        {
            return new(GetId(), true);
        }

        public override uint GetId()
        {
            return Id;
        }

        public override CharacterData ToCharacterData()
        {
            return base.ToCharacterData(Id, Character.GetName(), Character.GetLevel());
        }

        public override RuntimeAnimatorController GetHitAnimator()
        {
            return hitAnimator;
        }

        public float GetThinkingTime(FlashCard flashCard)
        {
            return GetComponent<EnemyAttackModule>().GetThinkingTime(flashCard);
        }

        public uint GetQuestionAnswer(FlashCard flashCard)
        {
            return GetComponent<EnemyAttackModule>().GetQuestionAnswer(flashCard);
        }

        public void Learn(FlashCard question, bool isCorrect, float answeredTime)
        {
            GetComponent<EnemyAttackModule>().Learn(question, isCorrect, answeredTime);
        }
    }
}