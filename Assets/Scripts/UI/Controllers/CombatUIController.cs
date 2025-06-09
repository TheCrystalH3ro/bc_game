using System.Collections.Generic;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.UI.Controllers
{
    public class CombatUIController : MonoBehaviour
    {
        public static CombatUIController Singleton { get; private set; }

        [SerializeField] private CharacterStatus playerStatus;
        [SerializeField] private CharacterStatus enemyStatus;

        [SerializeField] private RoundTimer roundTimer;
        [SerializeField] private ActionBar actionBar;

        void Awake()
        {
            Singleton = this;
        }

        public void LoadCharacter(PlayerController player)
        {
            LoadCharacter(playerStatus, player);
        }

        public void LoadCharacter(EnemyController enemy)
        {
            LoadCharacter(enemyStatus, enemy);
        }

        private void LoadCharacter(CharacterStatus characterStatus, BaseCharacterController character)
        {
            CharacterData data = character.ToCharacterData();
            characterStatus.AddCharacter(data);

            HealthModule healthModule = character.GetComponent<HealthModule>();
            characterStatus.RegisterHealthEvent(data.GetId(), healthModule);
        }

        public void SetRoundTime(int time)
        {
            roundTimer.SetMaxTime(time);
        }

        public void ChangeRoundTime(int time)
        {
            roundTimer.SetTime(time);
        }

        public void SetCharacterTurn(BaseCharacterController character)
        {
            actionBar.SetCharacterTurn(character);
        }

        public void SetCharacterAttack(BaseCharacterController character, BaseCharacterController target)
        {
            actionBar.SetCharacterAttack(character, target);
        }

        public void SetCharacterDeath(BaseCharacterController character)
        {
            actionBar.SetCharacterDeath(character);
        }

        public void OpenButtonsPanel()
        {
            actionBar.OpenButtons();
        }

        public void SetButtonsActive(bool active)
        {
            actionBar.SetButtonsActive(active);
        }

        public void SetQuestion(string question, Dictionary<uint, string> answers)
        {
            actionBar.SetQuestion(question, answers);
        }
    }
}