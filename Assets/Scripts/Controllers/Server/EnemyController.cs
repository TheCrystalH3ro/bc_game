using System;
using System.Linq;
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

        public NetworkObject Prefab;

        public IEnemy Character { get; private set; }

        public uint Id { get; private set; }

        public static uint lastId = 0;

        [SerializeField] private int BASE_DAMAGE = 10;

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
            return UnityEngine.Random.Range(0, flashCard.GetTime());
        }

        public uint GetQuestionAnswer(FlashCard flashCard)
        {
            int answers = flashCard.GetAnswers().Keys.Count;
            int answerIndex = UnityEngine.Random.Range(0, answers);

            return flashCard.GetAnswers().Keys.ElementAt(answerIndex);
        }

        public override int GetDamage(FlashCard flashCard, float remainingTime)
        {
            return BASE_DAMAGE;
        }
    }
}