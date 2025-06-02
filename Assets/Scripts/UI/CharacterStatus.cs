using System.Collections.Generic;
using Assets.Scripts.Models;
using Assets.Scripts.UI.Controllers;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class CharacterStatus : MonoBehaviour
    {
        [SerializeField] private GameObject characterStatusPrefab;

        private Dictionary<uint, GameObject> players = new();

        public void AddCharacter(CharacterData character)
        {
            GameObject characterStatus = Instantiate(characterStatusPrefab);

            CharacterStatusController characterStatusController = characterStatus.GetComponentInChildren<CharacterStatusController>();

            characterStatusController.Init(character);

            characterStatus.transform.SetParent(transform, false);

            players.Add(character.GetId(), characterStatus);
        }

        public void RemoveCharacter(uint characterId)
        {
            if (!players.ContainsKey(characterId)) return;

            GameObject playerStatus = players[characterId];
            Destroy(playerStatus);

            players.Remove(characterId);
        }

        public void RegisterHealthEvent(uint characterId, HealthModule healthModule)
        {
            GameObject characterStatus = players[characterId];

            CharacterStatusController characterStatusController = characterStatus.GetComponentInChildren<CharacterStatusController>();

            healthModule.OnHurt.AddListener(characterStatusController.UpdateHealth);
            healthModule.OnHeal.AddListener(characterStatusController.UpdateHealth);
        }
    }
}